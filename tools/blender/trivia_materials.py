"""Deterministic, image-backed PBR surfaces. No downloaded textures.

Images are evaluated from analytic surface functions into UV texture maps;
the Blender render uses the exact same maps supplied to Unity.
"""
import bpy, math
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
      'StageWood':((.265,.105,.044),.02,.30,'wood'),
      'Walnut':((.15,.042,.017),.02,.30,'wood'),
      'BurgundyWood':((.24,.025,.042),.02,.42,'wood'),
      'Gold':((.69,.43,.155),.88,.29,'metal'),
      'InlayGold':((.83,.52,.08),.28,.43,'metal'),
      'GoldDark':((.39,.23,.08),.85,.31,'metal'),
      'VelvetRed':((.255,.009,.026),0,.88,'cloth'),
      'VelvetSeat':((.23,.009,.032),0,.84,'cloth'),
      'RoyalPurple':((.16,.018,.285),.0,.72,'cloth'),
      'BuntingGold':((.68,.39,.085),.0,.72,'cloth'),
      'Ivory':((.59,.47,.33),.0,.48,'stone'),
      'Marble':((.73,.65,.51),.0,.26,'stone'),
      'BlackMetal':((.012,.014,.018),.65,.31,'metal'),
      'Monitor':((.013,.008,.024),.05,.24,'plain'),
      'Screen':((.88,.76,.56),0,.6,'plain'),
      'Leaves':((.035,.115,.018),0,.74,'leaf'),
      'PetalPurple':((.38,.10,.61),0,.65,'leaf'),
      'PetalCream':((.85,.72,.49),0,.68,'leaf'),
      'PetalWhite':((.85,.83,.74),0,.66,'leaf'),
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
            # Restrained longitudinal grain; avoid the old high-contrast swirls
            # and coarse normal ripples that looked like exposed raw timber.
            fine=np.sin(y*410+np.sin(x*8)*3+np.sin(x*27)*.6)
            pores=np.maximum(0,np.sin(y*1900+np.sin(x*13)*7)-.965)
            variation=1+grain*.035+slow*.10+fine*.014+noise*.012-pores*.5
            height=fine*.000008+noise*.000002
        elif kind=='cloth':
            # Tileable woven damask: staggered botanical medallions with
            # mirrored leaf forms. Colour remains subtle; relief catches light.
            yy=(y*4)%1;xx=(x*4+np.floor(y*4)*.5)%1
            xx=xx-.5;yy=yy-.5
            stem=np.exp(-((xx-.06*np.sin(yy*11))/.017)**2)*np.exp(-(yy/.36)**8)
            motif=stem*.4
            for sx in [-1,1]:
                for h in [-.19,.02,.20]:
                    px=xx-sx*.10;py=yy-h
                    ax=px*.8+sx*py*.6;ay=-sx*px*.6+py*.8
                    motif+=np.exp(-((ax/.095)**2+(ay/.037)**2)*2)
            motif=np.clip(motif,0,1)
            weave=np.sin(x*math.pi*2*640)*np.sin(y*math.pi*2*640)
            variation=1+slow*.035+noise*.018+motif*.11
            height=motif*.00010+weave*.000006
        elif kind=='metal':
            patina=np.clip((ss+.1)*1.5,0,1)
            brush=np.sin(y*2700+np.sin(x*17)*3) if n==size else nn
            variation=1-patina*.09+nn*.012+brush*.004
            height=nn*.000002
        elif kind=='stone':
            veins=np.exp(-np.abs(np.sin(x*19+y*10+np.sin(y*25)*.5))*35)
            hair=np.exp(-np.abs(np.sin(x*31+y*13+np.sin(y*17)*.35))*100)
            variation=1+slow*.035-veins*.045-hair*.065+noise*.008
            height=hair*.000005
        elif kind=='leaf':
            variation=1+ss*.14+nn*.04
            height=ss*.00008 if name=='Leaves' else np.zeros((n,n),np.float32)
        rgb=np.asarray(col,np.float32)[None,None,:]*variation[:,:,None]
        roughness=np.clip(rough+ss*.055+nn*.025,.08,.95)
        if kind=='metal':roughness=np.clip(rough+patina*.12+nn*.008,.08,.95)
        if kind=='wood':roughness=np.clip(rough+ss*.035+nn*.008,.08,.95)
        if kind=='cloth':roughness=np.clip(rough-motif*.055+nn*.012,.08,.97)
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
            bs.inputs['Sheen Weight'].default_value=.38;bs.inputs['Sheen Roughness'].default_value=.75
            bs.inputs['Sheen Tint'].default_value=(*col,1)
            bs.inputs['Specular IOR Level'].default_value=.10
        if kind=='wood': bs.inputs['Coat Weight'].default_value=.24; bs.inputs['Coat Roughness'].default_value=.24
        if 'Emission' in maps:
            node=nodes.new('ShaderNodeTexImage'); node.image=maps['Emission']
            links.new(node.outputs[0],bs.inputs['Emission Color'])
            bs.inputs['Emission Strength'].default_value=2.2 if name=='Bulb' else (1.3 if name=='Symbol' else .32)
        if name=='Screen': bs.inputs['Emission Color'].default_value=(*col,1); bs.inputs['Emission Strength'].default_value=.18
        result[name]=m
        manifest[m.name]={'maps':{k:Path(v.filepath_raw).name for k,v in maps.items()},'metallic':metal,'roughness':rough,'size':n,'emission_strength':bs.inputs['Emission Strength'].default_value}
    return result,manifest
