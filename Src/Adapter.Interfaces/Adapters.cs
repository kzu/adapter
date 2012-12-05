﻿#region BSD License
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

    /// <summary>
    /// Provides the entry point for setting the implementation of the 
    /// <see cref="IAdapterService"/> as well as the <see cref="As"/> 
    /// extension method for consumers.
    /// </summary>
    public static class Adapters
    {
        // AppDomain-wise global static service instance that is set with the same GUID from the AdapterService implementation.
        private static Lazy<IAdapterService> service = new Lazy<IAdapterService>(() => (IAdapterService)AppDomain.CurrentDomain.GetData(Constants.GlobalStateIdentifier));
        private static readonly AmbientSingleton<IAdapterService> transientService = new AmbientSingleton<IAdapterService>(new Guid(Constants.TransientStateIdenfier));

        /// <summary>
        /// Tries to adapt the given <paramref name="source"/> to the requested <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The adapted object if an adapter for the source could be found; <see langword="null"/> otherwise.</returns>
        public static T As<T>(this object source)
            where T : class
        {
            return AdapterService.As<T>(source);
        }

        private static IAdapterService AdapterService
        {
            get
            {
                var implementation = transientService.Value ?? service.Value;
                if (implementation == null)
                    throw new InvalidOperationException("No adapter service has been set.");

                return implementation;
            }
        }
    }
}
