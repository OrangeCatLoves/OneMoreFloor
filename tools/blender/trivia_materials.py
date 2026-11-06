"""Deterministic, image-backed PBR surfaces. No downloaded textures.

Images are evaluated from analytic surface functions into UV texture maps;
the Blender render uses the exact same maps supplied to Unity.
"""
import bpy
import numpy as np
from pathlib import Path

def save_map(path, rgb, colorspace='sRGB', alpha=None):
    h,w=rgb.shape[:2]
    rgba=np.ones((h,w,4),dtype=np.float32)
    rgba[:,:,:3]=np.clip(rgb,0,1)
    if alpha is not None: rgba[:,:,3]=alpha
    img=bpy.data.images.new(path.stem,width=w,height=h,alpha=True)
    img.colorspace_settings.name=colorspace
    img.pixels.foreach_set(rgba.ravel())
    img.filepath_raw=str(path); img.file_format='PNG'; img.save()
    return img

def build_materials(folder, size=2048, refresh=False):
    folder=Path(folder); folder.mkdir(parents=True,exist_ok=True)
    rng=np.random.default_rng(62107)
    y,x=np.mgrid[0:size,0:size].astype(np.float32)/size
    noise=rng.random((size,size),dtype=np.float32)-.5
    slow=(np.sin(x*37+np.sin(y*23))*np.sin(y*41+x*8)+np.sin(y*73+x*57))*.25
    grain=np.sin(y*140+np.sin(x*12)*4+np.sin(x*31)*.8)
    specs={
      'StageWood':((.33,.145,.065),.04,.35,'wood'),
      'Walnut':((.15,.042,.017),.02,.30,'wood'),
      'BurgundyWood':((.24,.025,.042),.02,.42,'wood'),
      'Gold':((.77,.49,.18),.90,.27,'metal'),
      'InlayGold':((.83,.52,.08),.28,.43,'metal'),
      'GoldDark':((.39,.23,.08),.85,.31,'metal'),
      'VelvetRed':((.37,.012,.037),0,.82,'cloth'),
      'VelvetSeat':((.34,.012,.043),0,.79,'cloth'),
      'RoyalPurple':((.26,.025,.42),.02,.62,'cloth'),
      'Ivory':((.59,.47,.33),.0,.48,'stone'),
      'Marble':((.73,.65,.51),.0,.26,'stone'),
      'BlackMetal':((.012,.014,.018),.65,.31,'metal'),
      'Monitor':((.013,.008,.024),.05,.24,'plain'),
      'Screen':((.88,.76,.56),0,.6,'plain'),
      'Leaves':((.055,.15,.032),0,.64,'leaf'),
      'PetalPurple':((.31,.055,.48),0,.58,'leaf'),
      'PetalCream':((.85,.65,.37),0,.55,'leaf'),
      'PetalWhite':((.81,.75,.66),0,.56,'leaf'),
      'Bulb':((1,.64,.22),0,.22,'emission'),
      'Red':((.7,.006,.015),.08,.22,'button'),
      'Blue':((.005,.10,.75),.08,.22,'button'),
      'Yellow':((.95,.5,.005),.08,.22,'button'),
      'Green':((.004,.44,.04),.08,.22,'button'),
      'Symbol':((.9,.89,.68),0,.25,'emission'),
    }
    result={}; manifest={}
    for name,(col,metal,rough,kind) in specs.items():
        n=size if kind not in ('plain','emission','button') else 64
        xx=x[:n,:n] if n==size else np.mgrid[0:n,0:n][1]/n
        nn=noise[:n,:n]; ss=slow[:n,:n]
        variation=np.ones((n,n),np.float32)
        height=np.zeros((n,n),np.float32)
        if kind=='wood':
            variation=1+grain*.10+slow*.14+noise*.04
            height=grain*.00012+noise*.000015
            # Fine elongated pores and very restrained hairline wear.
            variation-=np.maximum(0,np.sin(y*1720+np.sin(x*16)*6)-.94)*1.2
        elif kind=='cloth':
            weave=np.sin(x*size*np.pi)*np.sin(y*size*np.pi)
            damask=np.sin(x*60+2*np.sin(y*36))*np.sin(y*36)
            variation=1+slow*.04+noise*.05+damask*.018
            height=damask*.00003+noise*.00001
        elif kind=='metal':
            variation=1+ss*.015+nn*.025
            height=nn*.000009
        elif kind=='stone':
            veins=np.exp(-np.abs(np.sin(x*19+y*10+np.sin(y*25)*.5))*35)
            variation=1+slow*.08-veins*.10+noise*.02
            height=veins*.0005
        elif kind=='leaf':
            variation=1+ss*.14+nn*.04
            height=ss*.001
        rgb=np.asarray(col,np.float32)[None,None,:]*variation[:,:,None]
        roughness=np.clip(rough+ss*.055+nn*.025,.08,.95)
        if kind=='metal':roughness=np.clip(rough+ss*.008+nn*.015,.08,.95)
        if np.ndim(height)==0: height=np.zeros((n,n),np.float32)
        dy,dx=np.gradient(height)
        normal=np.stack((-dx*n*.6,-dy*n*.6,np.ones((n,n),np.float32)),axis=-1)
        normal/=np.linalg.norm(normal,axis=-1,keepdims=True)
        maps={}
        arrays={'BaseColor':(rgb,'sRGB',None),
                'Normal':(normal*.5+.5,'Non-Color',None),
                'MetallicSmoothness':(np.full((n,n,3),metal,np.float32),'Non-Color',1-roughness),
                'Roughness':(np.repeat(roughness[:,:,None],3,axis=2),'Non-Color',None)}
        if kind in ('emission','button'):
            arrays['Emission']=(np.broadcast_to(np.asarray(col),(n,n,3)),'sRGB',None)
        for suffix,(pixels,space,a) in arrays.items():
            p=folder/f'T_Trivia_{name}_{suffix}.png'
            if p.exists() and not refresh:
                img=bpy.data.images.load(str(p),check_existing=True); img.colorspace_settings.name=space
            else: img=save_map(p,pixels,space,a)
            maps[suffix]=img
        m=bpy.data.materials.new('M_Trivia_'+name); m.use_nodes=True
        nodes=m.node_tree.nodes; links=m.node_tree.links; bs=nodes.get('Principled BSDF')
        bs.inputs['Metallic'].default_value=metal
        for key,socket in [('BaseColor','Base Color'),('Roughness','Roughness')]:
            node=nodes.new('ShaderNodeTexImage'); node.image=maps[key]
            links.new(node.outputs['Color'],bs.inputs[socket])
        node=nodes.new('ShaderNodeTexImage'); node.image=maps['Normal']
        nm=nodes.new('ShaderNodeNormalMap'); links.new(node.outputs['Color'],nm.inputs['Color']); links.new(nm.outputs[0],bs.inputs['Normal'])
        if kind=='cloth':
            bs.inputs['Sheen Weight'].default_value=.22;bs.inputs['Sheen Roughness'].default_value=.6
            bs.inputs['Sheen Tint'].default_value=(*col,1)
            bs.inputs['Specular IOR Level'].default_value=.18
        if kind=='wood': bs.inputs['Coat Weight'].default_value=.22; bs.inputs['Coat Roughness'].default_value=.30
        if 'Emission' in maps:
            node=nodes.new('ShaderNodeTexImage'); node.image=maps['Emission']
            links.new(node.outputs[0],bs.inputs['Emission Color'])
            bs.inputs['Emission Strength'].default_value=3 if name=='Bulb' else (2 if name=='Symbol' else .5)
        if name=='Screen': bs.inputs['Emission Color'].default_value=(*col,1); bs.inputs['Emission Strength'].default_value=.18
        result[name]=m
        manifest[m.name]={'maps':{k:Path(v.filepath_raw).name for k,v in maps.items()},'metallic':metal,'roughness':rough,'size':n,'emission_strength':bs.inputs['Emission Strength'].default_value}
    return result,manifest
