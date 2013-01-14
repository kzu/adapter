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

namespace Patterns.Adapter
{
    using System;
    using Moq;
    using Xunit;
    using System.Linq;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.IO;
    using System.Text.RegularExpressions;

    public class AdaptersSpec
    {
        [Fact]
        public void WhenGlobalServiceSpecified_ThenExtensionMethodUsesIt()
        {
            var service = Mock.Of<IAdapterService>(x => x.Adapt(It.IsAny<IFoo>()) == Mock.Of<IAdaptable<IFoo>>());
            AdaptersInitializer.SetService(service);

            var adaptable = Mock.Of<IFoo>().Adapt();

            Assert.NotNull(adaptable);

            adaptable.As<IBar>();

            Mock.Get(service).Verify(x => x.Adapt(It.IsAny<IFoo>()));
            Mock.Get(adaptable).Verify(x => x.As<IBar>());
        }

        [Fact]
        public void WhenTransientServiceSpecified_ThenOverridesGlobalService()
        {
            var transient = Mock.Of<IAdapterService>(x => x.Adapt(It.IsAny<IFoo>()) == Mock.Of<IAdaptable<IFoo>>());

            using (AdaptersInitializer.SetTransientService(transient))
            {

                var adaptable = Mock.Of<IFoo>().Adapt();

                Assert.NotNull(adaptable);

                adaptable.As<IBar>();

                Mock.Get(transient).Verify(x => x.Adapt(It.IsAny<IFoo>()));
                Mock.Get(adaptable).Verify(x => x.As<IBar>());
            }
        }

        public interface IFoo { }
        public interface IBar { }
    }
}
