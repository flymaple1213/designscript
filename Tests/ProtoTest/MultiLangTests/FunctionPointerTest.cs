using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.MultiLangTests
{
    class FunctionPointerTest
    {
        public ProtoCore.Core core;
        public TestFrameWork thisTest = new TestFrameWork();
        string testCasePath = "..\\..\\..\\Scripts\\MultiLangTests\\FunctionPointerTest\\";
        [SetUp]
        public void Setup()
        {
            core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
        }

        [Test]
        public void T01_BasicGlobalFunction()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
a = foo;
b = foo(3); //b=3;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 3;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T02_GlobalFunctionWithDefaultArg()
        {
            string code = @"
def foo:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a = foo;
b = foo(3); //b=5.0;
c = foo(2, 4.0); //c = 6.0";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T03_GlobalFunctionInAssocBlk()
        {
            string code = @"
a;
b;
c;
[Associative]
{
	def foo:double(x:int, y:double = 2.0)
	{
		return = x + y;
	}
	a = foo;
	b = foo(3); //b=5.0;
	c = foo(2, 4.0); //c = 6.0
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T04_GlobalFunctionInImperBlk()
        {
            string code = @"
a;
b;
c;
[Imperative]
{
	def foo:double(x:int, y:double = 2.0)
	{
		return = x + y;
	}
	a = foo;
	b = foo(3); //b=5.0;
	c = foo(2, 4.0); //c = 6.0
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T05_ClassMemerVarAsFunctionPointer()
        {
            string code = @"
class A
{
	x:var;
	constructor A()
	{
		x = foo;
	}
}
def foo:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a = A.A();
b = a.x(3,2.0);	//b=5.0;
c = a.x(2, 4.0);	//c = 6.0";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T05_ClassMemerVarAsFunctionPointerDefaultArg()
        {
            string code = @"
class A
{
	x:var;
	constructor A()
	{
		x = foo;
	}
}
def foo:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a = A.A();
b = a.x(3,2.0);	//b=5.0;
c = a.x(2, 4.0);	//c = 6.0";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T06_ClassMemerVarAsFunctionPointerAssocBlk()
        {
            string code = @"
class A
{
	x:var;
	constructor A()
	{
		x = foo;
	}
}
def foo:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a;
b;
c;
[Associative]
{
	a = A.A();
	b = a.x(3,2.0);	//b=5.0;
	c = a.x(2, 4.0);	//c = 6.0
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T07_ClassMemerVarAsFunctionPointerImperBlk()
        {
            string code = @"
class A
{
	x:var;
	constructor A()
	{
		x = foo;
	}
}
def foo:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a;
b;
c;
[Imperative]
{
	a = A.A();
	b = a.x(3);	//b=5.0;
	c = a.x(2, 4.0);	//c = 6.0
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T08_FunctionPointerUpdateTest()
        {
            string code = @"
def foo1:int(x:int)
{
	return = x;
}
def foo2:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a = foo1;
b = a(3);
a = foo2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T09_NegativeTest_Non_FunctionPointer()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
a = 2;
b = a();";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = null;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T10_NegativeTest_UsingFunctionNameAsVarName_Global()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
def foo:int(x:int)
{
	return = x;
}
foo = 3;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T11_NegativeTest_UsingFunctionNameAsVarName_Global_ImperBlk()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Imperative]
{
	def foo:int(x:int)
	{
		return = x;
	}
	foo = 3;
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T12_NegativeTest_UsingGlobalFunctionNameAsMemVarName_Class()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
def foo:int(x:int)
{
	return = x;
}
class A
{
	foo : var;
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T13_NegativeTest_UsingMemFunctionNameAsMemVarName_Class()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
def foo:int(x:int)
{
	return = x;
}
class A
{
	memFoo : var;
	def memFoo()
	{
		return = 2;
	}
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("Negative")]
        public void T14_NegativeTest_UsingFunctionNameInNonAssignBinaryExpr()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
def foo:int(x:int)
{
	return = x;
}
a = foo + 2;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T15_NegativeTest_UsingFunctionNameInNonAssignBinaryExpr_Global_ImperBlk()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Imperative]
{
	def foo:int(x:int)
	{
		return = x;
	}
	a = foo + 2;
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T16_NegativeTest_UsingMemFunctionAsFunctionPtr()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
class A
{
	x : function; 
	y: function;
	constructor A()
	{
		x = foo;
		y = memFoo;
	}
	def memFoo(xx:int)
	{
		return = xx;
	}
}
a = A.A();
x = a.x(2);
y = a.y(2);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object x = 2;
            object y = null;
            thisTest.Verify("x", x);
            thisTest.Verify("y", y);
        }

        [Test]
        public void T17_PassFunctionPointerAsArg()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
def foo1:int(f:function, x:int)
{
	return = f(x);
}
a = foo1(foo, 2);
b = foo;
c = foo1(b, 3);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = 2;
            object c = 3;
            thisTest.Verify("a", a);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T18_FunctionPointerAsReturnVal()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
def foo1:int(f : function, x:int)
{
	return = f(x);
}
def foo2:function()
{
	return = foo;
}
a = foo2();
b = a(2);
c = foo1(a, 3);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 2;
            object c = 3;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T19_NegativeTest_PassingFunctionPtrAsArg_CSFFI()
        {
            string code = @"
import (""ProtoGeometry.dll"");
def foo : CoordinateSystem()
{
	return = CoordinateSystem.Identity();
}
a = Point.ByCartesianCoordinates(foo, 1.0, 2.0, 3.0);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("a", a);
        }

        [Test]
        public void T20_FunctionPtrUpdateOnMemVar_1()
        {
            string code = @"
def foo1:int(x:int)
{
	return = x;
}
class A
{
	x:function;
	constructor A(xx:function)
	{
		x = xx;
	}
}
a = A.A(foo1);
b = a.x(3);    //b = 3";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 3;
            thisTest.Verify("b", b);
        }

        [Test]
        [Category("Method Resolution")]
        public void T21_FunctionPtrUpdateOnMemVar_2()
        {
            string code = @"
def foo1:int(x:int)
{
	return = x;
}
def foo2:double(x:int, y:double = 2.0)
{
	return = x + y;
}
class A
{
	x:var;
}
a = A.A();
a.x = foo1;
b = a.x(2); //b = 3;
a.x = foo2; //b = 5.0";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T22_FunctionPointer_Update()
        {
            string code = @"class A{x;}def foo(x){    return = 2 * x;}def bar(x, f){    return = f(x);}x = 100;a = A.A();a.x = x;x = bar(x, foo);t = a.x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", 200);
        }

        [Test]
        public void T22_FunctionPointerArray()
        {
            string code = @"def foo(x){    return = 2 * x;}fs = {foo, foo};r = fs[0](100);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T23_FunctionPointerAsReturnValue()
        {
            string code = @"def foo(x){    return = 2 * x;}def bar(i){    return = foo;}fs = bar(0..1);f = fs[0];r = f(100);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T24_FunctionPointerAsReturnValue2()
        {
            string code = @"def foo(x){    return = 2 * x;}def bar:function[](){    return = {foo, foo};}fs = bar();f = fs[0];r = f(100);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T25_FunctionPointerTypeConversion()
        {
            string code = @"def foo:int(x){    return = 2 * x;}def bar:var[](){    return = {foo, foo};}fs = bar();f = fs[0];r = f(100);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T26_NestedFunctionPointer()
        {
            string code = @"def foo(x){    return = 2 * x;}def bar(x){    return = 3 * x;}def ding(x, f1:var, f2:var){    return = f1(f2(x));}x = 1;r = ding(x, foo, bar);x = 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 12);
        }

        [Test]
        public void T27_FunctionPointerDefaultParameter()
        {
            string code = @"def foo(x, y = 10, z = 100){    return = x + y + z;}def bar(x, f){    return = f(x);}r = bar(1, foo);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 111);
        }

        [Test]
        public void T28_FunctionPointerInInlineCond()
        {
            string code = @"def foo(x, y = 10, z = 100){    return = x + y + z;}def bar(x, y = 2, z = 3){    return = x * y * z;}def ding(i, f, b){    return = (i > 0) ? f(i) : b(i);}r1 = ding(1, foo, bar);r2 = ding(-1, foo, bar);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 111);
            thisTest.Verify("r2", -6);
        }

        [Test]
        public void T29_FunctionPointerInInlineCond()
        {
            string code = @"def foo(x, y = 10, z = 100){    return = x + y + z;}def ding(i, f){    return = (i > 0) ? f(i) : f;}r1 = ding(1, foo);r2 = ding(-1, 100);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 111);
            thisTest.Verify("r2", 100);
        }

        [Test]
        public void T30_TypeConversion()
        {
            string code = @"def foo(){    return = null;}t1:int = foo;t2:int[] = foo;t3:char = foo;t4:string = foo;t5:bool = foo; t6:function = foo;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", null);
            thisTest.Verify("t2", new object[] { null });
            thisTest.Verify("t3", null);
            thisTest.Verify("t4", null);
            thisTest.Verify("t5", null);
        }

        [Test]
        public void T31_UsedAsMemberVariable()
        {
            string code = @"class A{    f;    x;    constructor A(_x, _f)    {        x = _x;        f = _f;    }    def update()    {        x = f(x);        return = null;    }}def foo(x){    return = 2 * x;}def bar(x){    return = 3 * x;}a = A.A(2, foo);r = a.update1();a.f = bar;r = a.x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 12);
        }
    }
}
