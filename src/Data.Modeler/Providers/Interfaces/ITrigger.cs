/*
Copyright 2017 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using Data.Modeler.Providers.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Modeler.Providers.Interfaces
{
    /// <summary>
    /// Trigger interface
    /// </summary>
    public interface ITrigger
    {
        /// <summary>
        /// Definition
        /// </summary>
        string Definition { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Parent table
        /// </summary>
        ITable ParentTable { get; set; }

        /// <summary>
        /// Trigger type
        /// </summary>
        TriggerType Type { get; set; }
    }
}