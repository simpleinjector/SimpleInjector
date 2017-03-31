namespace SimpleInjector.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Internals;

    [TestClass]
    public class InstanceProducerInlineVisualizerTests
    {
        [TestMethod]
        public void VisualizeInline_WithMaxLengthLongerThanTotalLengthOfGraph_ReturnsTheWholeGraph()
        {
            AssertGraph(82, "Consumer(Dep1(FirstSub(), SecondSub(), ThirdSub()), Dep2(FirstSub(), SecondSub()))");
        }

        [TestMethod]
        public void VisualizeInline_WithMaxLengthShorterThanCompleteGraph_ReturnsCorrectTruncated1()
        {
            AssertGraph(81, "Consumer(Dep1(FirstSub(), SecondSub(), ThirdSub()), Dep2(FirstSub(), ...))");
        }

        [TestMethod]
        public void VisualizeInline_WithMaxLengthShorterThanCompleteGraph_ReturnsCorrectTruncated2()
        {
            AssertGraph(74, "Consumer(Dep1(FirstSub(), SecondSub(), ThirdSub()), Dep2(FirstSub(), ...))");
        }

        [TestMethod]
        public void VisualizeInline_WithMaxLengthShorterThanCompleteGraph_ReturnsCorrectTruncated4()
        {
            AssertGraph(73, "Consumer(Dep1(FirstSub(), SecondSub(), ThirdSub()), Dep2(...))");
        }

        [TestMethod]
        public void VisualizeInline_WithMaxLengthShorterThanCompleteGraph_ReturnsCorrectTruncated5()
        {
            AssertGraph(62, "Consumer(Dep1(FirstSub(), SecondSub(), ThirdSub()), Dep2(...))");
        }

        [TestMethod]
        public void VisualizeInline_WithMaxLengthShorterThanCompleteGraph_ReturnsCorrectTruncated6()
        {
            AssertGraph(61, "Consumer(Dep1(FirstSub(), SecondSub(), ThirdSub()), ...)");
        }

        [TestMethod]
        public void VisualizeInline_WithMaxLengthShorterThanCompleteGraph_ReturnsCorrectTruncated7()
        {
            AssertGraph(56, "Consumer(Dep1(FirstSub(), SecondSub(), ThirdSub()), ...)");
        }

        [TestMethod]
        public void VisualizeInline_WithMaxLengthShorterThanCompleteGraph_ReturnsCorrectTruncated8()
        {
            AssertGraph(55, "Consumer(Dep1(FirstSub(), SecondSub(), ...), Dep2(...))");
        }

        [TestMethod]
        public void VisualizeInline_WithMaxLengthShorterThanCompleteGraph_ReturnsCorrectTruncated9()
        {
            AssertGraph(49, "Consumer(Dep1(FirstSub(), SecondSub(), ...), ...)");
        }

        [TestMethod]
        public void VisualizeInline_WithMaxLengthShorterThanCompleteGraph_ReturnsCorrectTruncated10()
        {
            AssertGraph(48, "Consumer(Dep1(FirstSub(), ...), Dep2(...))");
        }

        [TestMethod]
        public void VisualizeInline_WithMaxLengthShorterThanCompleteGraph_ReturnsCorrectTruncated11()
        {
            AssertGraph(36, "Consumer(Dep1(FirstSub(), ...), ...)");
        }

        [TestMethod]
        public void VisualizeInline_WithMaxLengthShorterThanCompleteGraph_ReturnsCorrectTruncated12()
        {
            AssertGraph(35, "Consumer(Dep1(...), Dep2(...))");
        }

        [TestMethod]
        public void VisualizeInline_WithMaxLengthShorterThanCompleteGraph_ReturnsCorrectTruncated13()
        {
            AssertGraph(24, "Consumer(Dep1(...), ...)");
        }

        [TestMethod]
        public void VisualizeInline_WithMaxLengthShorterThanCompleteGraph_ReturnsCorrectTruncated14()
        {
            AssertGraph(23, "Consumer(...)");
        }

        private static void AssertGraph(int maxLength, string expectedGraph)
        {
            // Arrange
            var producer = GetInstanceProducerForConsumer();

            // Act
            string actualGraph = producer.VisualizeInlinedAndTruncatedObjectGraph(maxLength);

            // Assert
            Assert.IsTrue(actualGraph.Length <= maxLength, "Graph too long: " + actualGraph);
            Assert.AreEqual(expectedGraph, actualGraph);
        }

        private static InstanceProducer GetInstanceProducerForConsumer()
        {
            var container = new Container();

            container.Register<Consumer>();

            container.Verify();

            var registration = container.GetRegistration(typeof(Consumer));
            return registration;
        }
    }
}