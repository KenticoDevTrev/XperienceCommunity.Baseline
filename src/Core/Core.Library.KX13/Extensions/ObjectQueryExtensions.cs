using CMS.DataEngine;
using CMS.DocumentEngine.Routing;
using CMS.Relationships;
using System.Data;

namespace Core.Extensions
{
    public static class ObjectQueryExtensions
    {
        public static DocumentQuery ColumnsNullHandled(this DocumentQuery baseQuery, string[] Columns)
        {
            if (Columns == null)
            {
                return baseQuery;
            }
            else
            {
                return baseQuery.SelectColumnsList.AnyColumnsDefined ? baseQuery.AddColumns(Columns) : baseQuery.Columns(Columns);
            }
        }

        public static DocumentQuery<TDocument> ColumnsNullHandled<TDocument>(this DocumentQuery<TDocument> baseQuery, string[] Columns) where TDocument : TreeNode, new()
        {
            if (Columns == null)
            {
                return baseQuery;
            }
            else
            {
                return baseQuery.SelectColumnsList.AnyColumnsDefined ? baseQuery.AddColumns(Columns) : baseQuery.Columns(Columns);
            }
        }

        public static MultiDocumentQuery ColumnsNullHandled(this MultiDocumentQuery baseQuery, string[] Columns)
        {
            if (Columns == null)
            {
                return baseQuery;
            }
            else
            {
                return baseQuery.SelectColumnsList.AnyColumnsDefined ? baseQuery.AddColumns(Columns) : baseQuery.Columns(Columns);
            }
        }

        public static ObjectQuery ColumnsNullHandled(this ObjectQuery baseQuery, string[] Columns)
        {
            if (Columns == null)
            {
                return baseQuery;
            }
            else
            {
                return baseQuery.SelectColumnsList.AnyColumnsDefined ? baseQuery.AddColumns(Columns) : baseQuery.Columns(Columns);
            }
        }

        public static ObjectQuery<TObject> ColumnsNullHandled<TObject>(this ObjectQuery<TObject> baseQuery, string[] Columns) where TObject : BaseInfo, new()
        {
            if (Columns == null)
            {
                return baseQuery;
            }
            else
            {
                return baseQuery.SelectColumnsList.AnyColumnsDefined ? baseQuery.AddColumns(Columns) : baseQuery.Columns(Columns);
            }
        }

        private static readonly string[] _pageIdentityColumns = new string[] {
                        nameof(TreeNode.NodeID),
                        nameof(TreeNode.DocumentID),
                        nameof(TreeNode.NodeGUID),
                        nameof(TreeNode.DocumentGUID),
                        nameof(TreeNode.NodeAlias),
                        nameof(TreeNode.NodeAliasPath),
                        nameof(TreeNode.DocumentCulture),
                        nameof(TreeNode.DocumentName),
                        nameof(TreeNode.NodeLevel),
                        nameof(TreeNode.NodeSiteID)
                        };

        public static DocumentQuery IncludePageIdentityColumns(this DocumentQuery baseQuery)
        {
            return baseQuery.SelectColumnsList.AnyColumnsDefined ?
                        baseQuery.AddColumns(_pageIdentityColumns).WithPageUrlPaths()
                        :
                        baseQuery.Columns(_pageIdentityColumns).WithPageUrlPaths();
        }

        public static DocumentQuery<TDocument> IncludePageIdentityColumns<TDocument>(this DocumentQuery<TDocument> baseQuery) where TDocument : TreeNode, new()
        {
            return baseQuery.SelectColumnsList.AnyColumnsDefined ?
                    baseQuery.AddColumns(_pageIdentityColumns).WithPageUrlPaths()
                    :
                    baseQuery.Columns(_pageIdentityColumns).WithPageUrlPaths();
        }

        public static MultiDocumentQuery IncludePageIdentityColumns(this MultiDocumentQuery baseQuery)
        {
            return baseQuery.SelectColumnsList.AnyColumnsDefined ?
                        baseQuery.AddColumns(_pageIdentityColumns).WithPageUrlPaths()
                        :
                        baseQuery.Columns(_pageIdentityColumns).WithPageUrlPaths();
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