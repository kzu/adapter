#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Adapter
{
    using System;
    using Moq;
    using Xunit;

    public class AdapterServiceSpec
    {
        [Fact]
        public void WhenAdapterDoesNotImplementInterface_ThenNoOps()
        {
            var service = new AdapterService(new BadAdapter());

            Assert.Null(service.As<IFormattable>(new object()));
        }

        [Fact]
        public void WhenAdapterImplementsInterface_ThenItIsUsed()
        {
            var service = new AdapterService(new StringAdapter());
            var from = Mock.Of<IFrom>();

            var adapted = service.As<string>(from);

            Assert.NotNull(adapted);
            Assert.Equal("foo", adapted);
        }

        [Fact]
        public void WhenAdapterImplementsMultipleInterfaces_ThenItIsCalledForAll()
        {
            var mock = new Mock<IAdapter<IFrom, string>>();
            mock.Setup(x => x.Adapt(It.IsAny<IFrom>())).Returns("foo");
            mock.As<IAdapter<IFrom, ITo>>();

            var service = new AdapterService(mock.Object);
            var from = Mock.Of<IFrom>();

            service.As<string>(from);
            service.As<ITo>(from);

            mock.Verify(x => x.Adapt(from));
            mock.As<IAdapter<IFrom, ITo>>().Verify(x => x.Adapt(from));
        }

        [Fact]
        public void WhenNoAdapterExists_ThenReturnsDefaultValue()
        {
            var service = new AdapterService();
            var from = Mock.Of<IFrom>();

            var adapted = service.As<string>(from);

            Assert.Null(adapted);
        }

        [Fact]
        public void WhenAdapterExistsForBaseInterface_ThenReturnsAdaptedObject()
        {
            var service = new AdapterService(new StringAdapter());
            var foo = Mock.Of<IFrom3>();

            var adapted = service.As<string>(foo);

            Assert.Equal("foo", adapted);
        }

        [Fact]
        public void WhenAdapterExistsToDerivedInterface_ThenReturnsAdaptedObject()
        {
            var service = new AdapterService(
                Mock.Of<IAdapter<IFrom, ITo2>>(x => 
                    x.Adapt(It.IsAny<IFrom>()) == Mock.Of<ITo2>()));

            var adapted = service.As<ITo>(Mock.Of<IFrom>());

            Assert.NotNull(adapted);
        }

        [Fact]
        public void WhenAdapterExistsToMultipleDerivedInterfaces_ThenReturnsAdaptedObjectFromMostSpecific()
        {
            var service = new AdapterService(
                Mock.Of<IAdapter<IFrom, ITo2>>(x =>
                    x.Adapt(It.IsAny<IFrom>()) == Mock.Of<ITo2>()), 
                Mock.Of<IAdapter<IFrom, ITo3>>(x =>
                    x.Adapt(It.IsAny<IFrom>()) == Mock.Of<ITo3>()));

            var adapted = service.As<ITo>(Mock.Of<IFrom>());

            Assert.NotNull(adapted);
            Assert.IsAssignableFrom<ITo3>(adapted);
        }

        [Fact]
        public void WhenMultipleAdaptersExist_ThenReturnsAdaptedObjectFromClosestInheritance()
        {
            var service = new AdapterService(new IAdapter[]
            {
                Mock.Of<IAdapter<IFrom, ITo>>(x =>
                    x.Adapt(It.IsAny<IFrom>()) == Mock.Of<ITo>(_ => _.Name == "IFrom, ITo")),
                Mock.Of<IAdapter<IFrom3, ITo3>>(x =>
                    x.Adapt(It.IsAny<IFrom3>()) == Mock.Of<ITo3>(_ => _.Name == "IFrom3, ITo3")),
            });

            Assert.Equal("IFrom, ITo", service.As<ITo>(Mock.Of<IFrom>()).Name);
            Assert.Equal("IFrom, ITo", service.As<ITo>(Mock.Of<IFrom2>()).Name);
            Assert.Equal("IFrom3, ITo3", service.As<ITo>(Mock.Of<IFrom3>()).Name);
            // No registered adapter to ITo2 from IFrom or IFrom2
            Assert.Null(service.As<ITo2>(Mock.Of<IFrom>()));
            Assert.Null(service.As<ITo2>(Mock.Of<IFrom2>()));
            // But ITo3 is assignable to ITo2, so from IFrom3 we can adapt
            Assert.Equal("IFrom3, ITo3", service.As<ITo2>(Mock.Of<IFrom3>()).Name);
        }

        public class GivenThreeAdaptersInHierarchy
        {
            private IAdapterService service;

            public GivenThreeAdaptersInHierarchy()
            {
                this.service = new AdapterService(new IAdapter[] 
				{
					Mock.Of<IAdapter<IFrom, string>>(a => a.Adapt(It.IsAny<IFrom>()) == "from"),
					Mock.Of<IAdapter<IFrom2, string>>(a => a.Adapt(It.IsAny<IFrom2>()) == "from2"), 
					Mock.Of<IAdapter<IFrom3, string>>(a => a.Adapt(It.IsAny<IFrom3>()) == "from3"), 
				});
            }

            [Fact]
            public void WhenAdapterExists_ThenReturnsAdaptedObjectFromMostSpecificInterface()
            {
                var foo = Mock.Of<IFrom3>();

                var adapted = this.service.As<string>(foo);

                Assert.Equal("from3", adapted);
            }

            [Fact]
            public void WhenAdapterExists_ThenReturnsAdaptedObjectFromSpecificInterface()
            {
                var foo = Mock.Of<IFrom2>();

                var adapted = this.service.As<string>(foo);

                Assert.Equal("from2", adapted);
            }

            [Fact]
            public void WhenNoAdapterExists_ThenReturnsDefaultValue()
            {
                var foo = Mock.Of<ICloneable>();

                var adapted = this.service.As<string>(foo);

                Assert.Equal(default(string), adapted);
            }

            [Fact]
            public void WhenDirectlyConvertible_ThenReturnsSameObject()
            {
                var foo = new SupportsIFromITo();

                var from = this.service.As<IFrom>(foo);
                var to = this.service.As<ITo>(foo);

                Assert.Same(foo, from);
                Assert.Same(foo, to);
            }
        }

        public class GivenAConcreteTypeAdapter
        {
            private IAdapterService service;

            public GivenAConcreteTypeAdapter()
            {
                this.service = new AdapterService(new IAdapter[] 
				{
						new ConcreteFromAdapter(), 
						Mock.Of<IAdapter<IFrom, string>>(a => a.Adapt(It.IsAny<IFrom>()) == "from"), 
						Mock.Of<IAdapter<IFrom2, string>>(a => a.Adapt(It.IsAny<IFrom2>()) == "from2"), 
						Mock.Of<IAdapter<IFrom3, string>>(a => a.Adapt(It.IsAny<IFrom3>()) == "from3"), 
				});
            }

            [Fact]
            public void WhenConcreteAdapterExists_ThenSelectsConcreteTypeAdapterFirst()
            {
                var from = new From();

                var adapted = this.service.As<string>(from);

                Assert.Equal("foo", adapted);
            }

            public class ConcreteFromAdapter : IAdapter<From, string>
            {
                public string Adapt(From from)
                {
                    return "foo";
                }
            }

            public class From : IFrom3
            {
                public string Name { get; set; }
            }

            public interface IFoo { }
        }

        public class BadAdapter : IAdapter { }

        public class StringAdapter : IAdapter<IFrom, string>
        {
            public string Adapt(IFrom from)
            {
                return "foo";
            }
        }

        public class SupportsIFromITo : IFrom, ITo 
        {
            public string Name { get; set; }
        }

        public interface IFrom { string Name { get; } }
        public interface IFrom2 : IFrom { }
        public interface IFrom3 : IFrom2 { }

        public interface ITo { string Name { get; } }
        public interface ITo2 : ITo { }
        public interface ITo3 : ITo2 { }
    }
}