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
    using Microsoft.Build.Utilities;
using System.Collections.Generic;
    using System.Xml.Linq;

    public class AdaptersSpec
    {
        private TaskItem[] Items;

        [Fact]
        public void when_action_then_assert()
        {
            Items = new[] { new TaskItem(@"D:\Code\clarius\github\adapter\Src\Adapter.Interfaces\bin\..\packages.config", 
                new Dictionary<string, string>
                {
                    { "FullPath", @"D:\Code\clarius\github\adapter\Src\Adapter.Interfaces\packages.config" },
                    { "NuSpec", @"D:\Code\clarius\github\adapter\Src\Adapter.Interfaces\bin\Package.nuspec" },
                }) };

        //<packages>
        //  <package id="netfx-Guard" version="1.3.2.0" targetFramework="net40" />
        //  <package id="netfx-System.AmbientSingleton" version="1.1.0.0" targetFramework="net40" />
        //</packages>

        //<dependencies>
        //    <group targetFramework="net40">
        //        <dependency id="" version=""/>
        //    </group>
        //</dependencies>

            foreach (var item in this.Items)
            {
                var config = XDocument.Load(item.GetMetadata("FullPath"));
                var packages = config.Root.Elements()
                    .Select(x => new
                    {
                        Id = x.Attribute("id").Value,
                        Version = x.Attribute("version").Value,
                        TargetFramework = x.Attribute("targetFramework").Value
                    })
                    .GroupBy(x => x.TargetFramework)
                    .Select(x => string.Format(
"\r\n            <group targetFramework=\"{0}\">", x.Key) + 
                        string.Join("", x.Select(d => string.Format(
                            "\r\n                <dependency id=\"{0}\" version=\"{1}\"/>", d.Id, d.Version))) +
"\r\n            <\\group>");

                var dependencies = 
                    "        <dependencies>" + 
                                string.Join("", packages) +
                    "\r\n        </dependencies>";

                Console.WriteLine("Dependencies");
                Console.WriteLine(dependencies);
            }
        }

        [Fact]
        public void WhenGlobalServiceSpecified_ThenExtensionMethodUsesIt()
        {
            var service = Mock.Of<IAdapterService>();
            AdaptersInitializer.SetService(service);

            Mock.Of<IFoo>().As<IBar>();

            Mock.Get(service).Verify(x => x.As<IBar>(It.IsAny<IFoo>()));
        }

        [Fact]
        public void WhenTransientServiceSpecified_ThenOverridesGlobalService()
        {
            var transient = Mock.Of<IAdapterService>();

            using (AdaptersInitializer.SetTransientService(transient))
            {
                Mock.Of<IFoo>().As<IBar>();

                Mock.Get(transient).Verify(x => x.As<IBar>(It.IsAny<IFoo>()));
            }
        }

        public interface IFoo { }
        public interface IBar { }
    }
}
