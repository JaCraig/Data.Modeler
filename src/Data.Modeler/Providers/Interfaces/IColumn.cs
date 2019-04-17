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

using System.Collections.Generic;
using System.Data;

namespace Data.Modeler.Providers.Interfaces
{
    /// <summary>
    /// Column interface
    /// </summary>
    public interface IColumn
    {
        /// <summary>
        /// Auto increment?
        /// </summary>
        bool AutoIncrement { get; set; }

        /// <summary>
        /// Gets the computed column specification (if one exists).
        /// </summary>
        /// <value>The computed column specification (if one exists).</value>
        string ComputedColumnSpecification { get; }

        /// <summary>
        /// Data type
        /// </summary>
        DbType DataType { get; set; }

        /// <summary>
        /// Default value
        /// </summary>
        string Default { get; set; }

        /// <summary>
        /// Foreign keys
        /// </summary>
        List<IColumn> ForeignKey { get; }

        /// <summary>
        /// Index?
        /// </summary>
        bool Index { get; set; }

        /// <summary>
        /// Data length
        /// </summary>
        int Length { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Nullable?
        /// </summary>
        bool Nullable { get; set; }

        /// <summary>
        /// On Delete Cascade
        /// </summary>
        bool OnDeleteCascade { get; set; }

        /// <summary>
        /// On Delete Set Null
        /// </summary>
        bool OnDeleteSetNull { get; set; }

        /// <summary>
        /// On Update Cascade
        /// </summary>
        bool OnUpdateCascade { get; set; }

        /// <summary>
        /// Parent table
        /// </summary>
        ITable ParentTable { get; set; }

        /// <summary>
        /// Primary key?
        /// </summary>
        bool PrimaryKey { get; set; }

        /// <summary>
        /// Unique?
        /// </summary>
        bool Unique { get; set; }

        /// <summary>
        /// Add foreign key
        /// </summary>
        /// <param name="foreignKeyTable">Table of the foreign key</param>
        /// <param name="foreignKeyColumn">Column of the foreign key</param>
        void AddForeignKey(string foreignKeyTable, string foreignKeyColumn);

        /// <summary>
        /// Copies this instance
        /// </summary>
        /// <param name="parentTable">The new parent table.</param>
        /// <returns>The copy</returns>
        IColumn Copy(ITable parentTable);

        /// <summary>
        /// Sets up the foreign key list
        /// </summary>
        void SetupForeignKeys();
    }
}