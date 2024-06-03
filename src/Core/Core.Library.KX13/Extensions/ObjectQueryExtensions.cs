using CMS.DataEngine;
using CMS.DocumentEngine.Routing;
using CMS.Relationships;
using System.Data;

namespace Core.Extensions
{
    public static class ObjectQueryExtensions
    {
        /// <summary>
        /// Use in place of .Columns()/.AddColumns().  Safetly either Sets the columns (if none set yet), or adds the columns (if columns are defined).
        /// </summary>
        /// <param name="baseQuery"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DocumentQuery ColumnsSafe(this DocumentQuery baseQuery, params string[] columns) => baseQuery.SelectColumnsList.AnyColumnsDefined ? baseQuery.AddColumns(columns ?? []) : baseQuery.Columns(columns ?? []);

        [Obsolete("Use ColumnsSafe instead")]
        public static DocumentQuery ColumnsNullHandled(this DocumentQuery baseQuery, string[] Columns) => baseQuery.ColumnsSafe(Columns);

        /// <summary>
        /// Use in place of .Columns()/.AddColumns().  Safetly either Sets the columns (if none set yet), or adds the columns (if columns are defined).
        /// </summary>
        /// <param name="baseQuery"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DocumentQuery<TDocument> ColumnsSafe<TDocument>(this DocumentQuery<TDocument> baseQuery, params string[] columns) where TDocument : TreeNode, new() => baseQuery.SelectColumnsList.AnyColumnsDefined ? baseQuery.AddColumns(columns ?? []) : baseQuery.Columns(columns ?? []);
        public static DocumentQuery<TDocument> ColumnsNullHandled<TDocument>(this DocumentQuery<TDocument> baseQuery, string[] Columns) where TDocument : TreeNode, new() => baseQuery.ColumnsSafe(Columns);

        /// <summary>
        /// Use in place of .Columns()/.AddColumns().  Safetly either Sets the columns (if none set yet), or adds the columns (if columns are defined).
        /// </summary>
        /// <param name="baseQuery"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static MultiDocumentQuery ColumnsSafe(this MultiDocumentQuery baseQuery, params string[] columns) => baseQuery.SelectColumnsList.AnyColumnsDefined ? baseQuery.AddColumns(columns ?? []) : baseQuery.Columns(columns ?? []);

        [Obsolete("Use ColumnsSafe instead")]
        public static MultiDocumentQuery ColumnsNullHandled(this MultiDocumentQuery baseQuery, string[] Columns) => baseQuery.ColumnsSafe(Columns);

        /// <summary>
        /// Use in place of .Columns()/.AddColumns().  Safetly either Sets the columns (if none set yet), or adds the columns (if columns are defined).
        /// </summary>
        /// <param name="baseQuery"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static ObjectQuery ColumnsSafe(this ObjectQuery baseQuery, params string[] columns) => baseQuery.SelectColumnsList.AnyColumnsDefined ? baseQuery.AddColumns(columns ?? []) : baseQuery.Columns(columns ?? []);
        
        [Obsolete("Use ColumnsSafe instead")]
        public static ObjectQuery ColumnsNullHandled(this ObjectQuery baseQuery, string[] Columns) => baseQuery.ColumnsSafe(Columns);

        /// <summary>
        /// Use in place of .Columns()/.AddColumns().  Safetly either Sets the columns (if none set yet), or adds the columns (if columns are defined).
        /// </summary>
        /// <param name="baseQuery"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static ObjectQuery<TObject> ColumnsSafe<TObject>(this ObjectQuery<TObject> baseQuery, params string[] columns) where TObject : BaseInfo, new() => baseQuery.SelectColumnsList.AnyColumnsDefined ? baseQuery.AddColumns(columns ?? []) : baseQuery.Columns(columns ?? []);

        [Obsolete("Use ColumnsSafe instead")]
        public static ObjectQuery<TObject> ColumnsNullHandled<TObject>(this ObjectQuery<TObject> baseQuery, string[] Columns) where TObject : BaseInfo, new() => baseQuery.ColumnsSafe(Columns);


        private static readonly string[] _pageIdentityColumns = [
                        nameof(TreeNode.NodeID),
                        nameof(TreeNode.DocumentID),
                        nameof(TreeNode.NodeGUID),
                        nameof(TreeNode.DocumentGUID),
                        nameof(TreeNode.NodeAlias),
                        nameof(TreeNode.NodeAliasPath),
                        nameof(TreeNode.DocumentCulture),
                        nameof(TreeNode.DocumentName),
                        nameof(TreeNode.NodeLevel),
                        nameof(TreeNode.NodeSiteID),
                        nameof(TreeNode.ClassName)
                        ];
        // <summary>
        /// Includes the columns needed for the PageIdentity fields, including the WithPageUrlPaths() joins
        /// 
        /// SHOULD ALWAYS BE USED IN CONJUCTION WITH .ColumnsSafe() as this will either limit or add to the columns returned.
        /// 
        /// If you wish to return all columns, use .WithPageUrlPaths() only, as all columns will contain the PageIdentity columns.
        /// </summary>
        /// <param name="baseQuery"></param>
        /// <returns></returns>
        public static DocumentQuery IncludePageIdentityColumns(this DocumentQuery baseQuery)
        {
            return baseQuery.ColumnsSafe(_pageIdentityColumns).WithPageUrlPaths();
        }

        // <summary>
        /// Includes the columns needed for the PageIdentity fields, including the WithPageUrlPaths() joins
        /// 
        /// SHOULD ALWAYS BE USED IN CONJUCTION WITH .ColumnsSafe() as this will either limit or add to the columns returned.
        /// 
        /// If you wish to return all columns, use .WithPageUrlPaths() only, as all columns will contain the PageIdentity columns.
        /// </summary>
        /// <param name="baseQuery"></param>
        /// <returns></returns>
        public static DocumentQuery<TDocument> IncludePageIdentityColumns<TDocument>(this DocumentQuery<TDocument> baseQuery) where TDocument : TreeNode, new()
        {
            return baseQuery.ColumnsSafe(_pageIdentityColumns).WithPageUrlPaths();
        }

        // <summary>
        /// Includes the columns needed for the PageIdentity fields, including the WithPageUrlPaths() joins
        /// 
        /// SHOULD ALWAYS BE USED IN CONJUCTION WITH .ColumnsSafe() as this will either limit or add to the columns returned.
        /// 
        /// If you wish to return all columns, use .WithPageUrlPaths() only, as all columns will contain the PageIdentity columns.
        /// </summary>
        /// <param name="baseQuery"></param>
        /// <returns></returns>
        public static MultiDocumentQuery IncludePageIdentityColumns(this MultiDocumentQuery baseQuery)
        {
            return baseQuery.ColumnsSafe(_pageIdentityColumns).WithPageUrlPaths();
        }

        public static DocumentQuery InRelationshipWithMany(this DocumentQuery baseQuery, IEnumerable<int> nodeIDs, string relationshipName) => nodeIDs.Any() ? baseQuery.Where(GetManyRelationshipsWhereInternal(nodeIDs, relationshipName)) : baseQuery;
        public static DocumentQuery<TDocument> InRelationshipWithMany<TDocument>(this DocumentQuery<TDocument> baseQuery, IEnumerable<int> nodeIDs, string relationshipName) where TDocument : TreeNode, new() => nodeIDs.Any() ? baseQuery.Where(GetManyRelationshipsWhereInternal(nodeIDs, relationshipName)) : baseQuery;
        public static MultiDocumentQuery InRelationshipWithMany<TDocument>(this MultiDocumentQuery baseQuery, IEnumerable<int> nodeIDs, string relationshipName) => nodeIDs.Any() ? baseQuery.Where(GetManyRelationshipsWhereInternal(nodeIDs, relationshipName)) : baseQuery;

        private static string GetManyRelationshipsWhereInternal(IEnumerable<int> nodeIDs, string relationshipName)
        {
            return $"NodeID in (select {nameof(RelationshipInfo.RightNodeId)} from {RelationshipInfo.TYPEINFO.GetTableName()} R inner join {RelationshipNameInfo.TYPEINFO.GetTableName()} RN on R.{nameof(RelationshipInfo.RelationshipNameId)} = RN.{nameof(RelationshipNameInfo.RelationshipNameId)} where {nameof(RelationshipNameInfo.RelationshipName)} = '{SqlHelper.EscapeQuotes(relationshipName)}' and {nameof(RelationshipInfo.LeftNodeId)} in ({string.Join(",", nodeIDs)}))";
        }

        public static async Task<IEnumerable<DataRow>> GetEnumeratedDataRowResultsAsync<T>(this ObjectQuery<T> query) where T : BaseInfo
        {
            var reader = await query.ExecuteReaderAsync();
            var dataSet = DataReaderToDataSet(reader);
            return dataSet.Tables[0].Rows.Cast<DataRow>();
        }

        public static async Task<IEnumerable<DataRow>> GetEnumeratedDataRowResultsAsync(this ObjectQuery query)
        {
            var reader = await query.ExecuteReaderAsync();
            var dataSet = DataReaderToDataSet(reader);
            return dataSet.Tables[0].Rows.Cast<DataRow>();
        }

        public static async Task<DataSet> GetEnumeratedDataSetResultsAsync<T>(this ObjectQuery<T> query) where T : BaseInfo
        {
            var reader = await query.ExecuteReaderAsync();
            return DataReaderToDataSet(reader);
        }

        public static async Task<DataSet> GetEnumeratedDataSetResultsAsync(this ObjectQuery query)
        {
            var reader = await query.ExecuteReaderAsync();
            return DataReaderToDataSet(reader);
        }

        /// <summary>
        /// Converts a DbDataReader to a DataSet, handles multiple tables in return result.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static DataSet DataReaderToDataSet(IDataReader reader)
        {
            if (reader is null)
            {
                var emptyDs = new DataSet();
                emptyDs.Tables.Add(new DataTable());
                return emptyDs;
            }

            var ds = new DataSet();
            // read each data result into a datatable
            do
            {
                var table = new DataTable();
                table.Load(reader);
                ds.Tables.Add(table);
            } while (!reader.IsClosed);

            return ds;
        }
    }
}