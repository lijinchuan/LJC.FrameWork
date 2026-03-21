using LJC.FrameWork.CodeExpression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LJC.FrameWork.CodeExpression.Tests
{
    [TestClass]
    public class ExpressCodeSyntaxTests
    {
        [TestMethod]
        public void CallResult_ShouldReturnLastExpressionResult()
        {
            var code = "x:1; y:2; x+y";
            var ec = new ExpressCode(code);

            var result = ec.CallResult();

            Assert.IsNotNull(result);
            Assert.AreEqual(3D, Convert.ToDouble(result.Result));
        }

        [TestMethod]
        public void Parse_ShouldNotSplitSemicolonInsideString()
        {
            var code = "x:'a;b'; y:1; y";
            var ec = new ExpressCode(code);

            var result = ec.CallResult();

            Assert.IsNotNull(ec.ExpressTreesBack);
            Assert.AreEqual(3, ec.ExpressTreesBack.Count);
            Assert.IsNotNull(result);
            Assert.AreEqual(1D, Convert.ToDouble(result.Result));
        }

        [TestMethod]
        public void Parse_ShouldNotSplitInsideParenthesesAcrossNewLine()
        {
            var code = "x:(1+\n2); y:3; x+y";
            var ec = new ExpressCode(code);

            var result = ec.CallResult();

            Assert.IsNotNull(ec.ExpressTreesBack);
            Assert.AreEqual(3, ec.ExpressTreesBack.Count);
            Assert.AreEqual(6D, Convert.ToDouble(result.Result));
        }

        [TestMethod]
        public void Parse_ShouldTreatIfBlockAsSingleExpressionWhenContainsNewLines()
        {
            var code = "if 1=1 then 1 else 0 end\ny:2\ny";
            var ec = new ExpressCode(code);

            var result = ec.CallResult();

            Assert.IsNotNull(ec.ExpressTreesBack);
            Assert.AreEqual(3, ec.ExpressTreesBack.Count);
            Assert.AreEqual(2D, Convert.ToDouble(result.Result));
        }

        [TestMethod]
        public void Parse_ShouldHandleConsecutiveSeparatorsWithoutIndexError()
        {
            var code = "x:1;;\r\n\r\n y:2;\n\n x+y";
            var ec = new ExpressCode(code);

            var result = ec.CallResult();

            Assert.IsNotNull(result);
            Assert.AreEqual(3D, Convert.ToDouble(result.Result));
        }

        [TestMethod]
        public void Parse_ShouldThrowExpressErrorException_WhenMissingEnd()
        {
            var ex = ExpectException<ExpressErrorException>(() => new ExpressCode("if 1=1 then 1").CallResult());
            StringAssert.Contains(ex.Message, "if表达式错误");
        }

        [TestMethod]
        public void Parse_ShouldThrowExpressErrorException_WhenBracketNotClosed()
        {
            var ex = ExpectException<ExpressErrorException>(() => new ExpressCode("x:(1+2").CallResult());
            StringAssert.Contains(ex.Message, "缺少右)");
        }

        [TestMethod]
        public void Parse_ShouldThrow_WhenElseWithoutIf()
        {
            var ex = ExpectException<ExpressErrorException>(() => new ExpressCode("else 1 end").CallResult());
            StringAssert.Contains(ex.Message, "else缺少if条件");
        }

        [TestMethod]
        public void Parse_ShouldThrow_WhenElseWithoutThen()
        {
            var ex = ExpectException<ExpressErrorException>(() => new ExpressCode("if 1=1 else 1 end").CallResult());
            StringAssert.Contains(ex.Message, "else前要有then");
        }

        [TestMethod]
        public void Parse_ShouldThrow_WhenEndWithoutOpenBlock()
        {
            var ex = ExpectException<ExpressErrorException>(() => new ExpressCode("end").CallResult());
            StringAssert.Contains(ex.Message, "end匹配失败");
        }

        [TestMethod]
        public void Parse_ShouldThrow_WhenIfMissingThenButHasEnd()
        {
            var ex = ExpectException<ExpressErrorException>(() => new ExpressCode("if 1=1 1 end").CallResult());
            StringAssert.Contains(ex.Message, "缺少then");
        }

        [TestMethod]
        public void Parse_ShouldThrow_WhenStepWithoutFor()
        {
            var ex = ExpectException<ExpressErrorException>(() => new ExpressCode("step 1").CallResult());
            StringAssert.Contains(ex.Message, "step缺少for条件");
        }

        [TestMethod]
        public void Parse_ShouldThrow_WhenToWithoutFor()
        {
            var ex = ExpectException<ExpressErrorException>(() => new ExpressCode("to 10").CallResult());
            StringAssert.Contains(ex.Message, "to缺少for条件");
        }

        [TestMethod]
        public void Parse_ShouldThrow_WhenBeginWithoutFor()
        {
            var ex = ExpectException<ExpressErrorException>(() => new ExpressCode("begin 1 end").CallResult());
            StringAssert.Contains(ex.Message, "begin缺少for条件");
        }

        [TestMethod]
        public void Parse_ShouldThrow_WhenForMissingBegin()
        {
            var ex = ExpectException<ExpressErrorException>(() => new ExpressCode("for x:1 to 2 end").CallResult());
            StringAssert.Contains(ex.Message, "for表达式中缺少begin表达式");
        }

        [TestMethod]
        public void For_ShouldSupportStepBeforeTo()
        {
            var code = "sum:0; for i:1 step 1 to 3 begin sum:=sum+i end; i";
            var result = new ExpressCode(code).CallResult();

            Assert.AreEqual(4D, Convert.ToDouble(result.Result));
        }

        [TestMethod]
        public void For_ShouldSupportStepAfterTo()
        {
            var code = "sum:0; for i:1 to 3 step 1 begin sum:=sum+i end; i";
            var result = new ExpressCode(code).CallResult();

            Assert.AreEqual(4D, Convert.ToDouble(result.Result));
        }

        [TestMethod]
        public void For_ShouldSupportDescendingLoop()
        {
            var code = "sum:0; for i:3 to 1 step -1 begin sum:=sum+i end; sum";
            var result = new ExpressCode(code).CallResult();

            Assert.AreEqual(9D, Convert.ToDouble(result.Result));
        }

        [TestMethod]
        public void For_ShouldThrow_WhenStepIsZeroConstant()
        {
            var ex = ExpectException<ExpressErrorException>(() => new ExpressCode("for i:1 to 2 step 0 begin i:=i+1 end").CallResult());
            StringAssert.Contains(ex.Message, "step不能为0");
        }

        private static TException ExpectException<TException>(Action action) where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException ex)
            {
                return ex;
            }

            Assert.Fail("Expected exception: " + typeof(TException).FullName);
            return null;
        }
    }
}
