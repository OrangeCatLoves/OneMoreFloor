using System.Runtime.CompilerServices;

// Unity warns about a code module with no scripts, so this file keeps the module
// compiling until it has real code. It also lets the tests reach internal members.
[assembly: InternalsVisibleTo("OneMoreFloor.Tests.EditMode")]
