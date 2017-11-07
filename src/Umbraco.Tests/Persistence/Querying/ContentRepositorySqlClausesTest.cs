﻿using System;
using System.Diagnostics;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class ContentRepositorySqlClausesTest : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Can_Verify_Base_Clause()
        {
            var NodeObjectType = Constants.ObjectTypes.Document;

            var expected = new Sql();
            expected.Select("*")
                .From("[cmsDocument]")
                .InnerJoin("[cmsContentVersion]").On("[cmsDocument].[versionId] = [cmsContentVersion].[VersionId]")
                .InnerJoin("[uContent]").On("[cmsContentVersion].[ContentId] = [uContent].[nodeId]")
                .InnerJoin("[umbracoNode]").On("[uContent].[nodeId] = [umbracoNode].[id]")
                .Where("([umbracoNode].[nodeObjectType] = @0)", new Guid("c66ba18e-eaf3-4cff-8a22-41b16d66a972"));

            var sql = Sql();
            sql.SelectAll()
                .From<DocumentDto>()
                // fixme DocumentDto does not have VersionId anymore
                //.InnerJoin<ContentVersionDto>()
                //.On<DocumentDto, ContentVersionDto>(left => left.VersionId, right => right.VersionId)
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectType);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
            for (var i = 0; i < expected.Arguments.Length; i++)
            {
                Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
            }

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_Verify_Base_Where_Clause()
        {
            var NodeObjectType = Constants.ObjectTypes.Document;

            var expected = Sql();
            expected.SelectAll()
                .From("[cmsDocument]")
                .InnerJoin("[cmsContentVersion]").On("[cmsDocument].[versionId] = [cmsContentVersion].[VersionId]")
                .InnerJoin("[uContent]").On("[cmsContentVersion].[ContentId] = [uContent].[nodeId]")
                .InnerJoin("[umbracoNode]").On("[uContent].[nodeId] = [umbracoNode].[id]")
                .Where("([umbracoNode].[nodeObjectType] = @0)", new Guid("c66ba18e-eaf3-4cff-8a22-41b16d66a972"))
                .Where("([umbracoNode].[id] = @0)", 1050);

            var sql = Sql();
            sql.SelectAll()
                .From<DocumentDto>()
                // fixme DocumentDto does not have VersionId anymore
                //.InnerJoin<ContentVersionDto>()
                //.On<DocumentDto, ContentVersionDto>(left => left.VersionId, right => right.VersionId)
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectType)
                .Where<NodeDto>(x => x.NodeId == 1050);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
            for (int i = 0; i < expected.Arguments.Length; i++)
            {
                Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
            }

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_Verify_Base_Where_With_Version_Clause()
        {
            var NodeObjectType = Constants.ObjectTypes.Document;
            var versionId = new Guid("2b543516-a944-4ee6-88c6-8813da7aaa07");

            var expected = new Sql();
            expected.Select("*")
                .From("[cmsDocument]")
                .InnerJoin("[cmsContentVersion]").On("[cmsDocument].[versionId] = [cmsContentVersion].[VersionId]")
                .InnerJoin("[uContent]").On("[cmsContentVersion].[ContentId] = [uContent].[nodeId]")
                .InnerJoin("[umbracoNode]").On("[uContent].[nodeId] = [umbracoNode].[id]")
                .Where("([umbracoNode].[nodeObjectType] = @0)", new Guid("c66ba18e-eaf3-4cff-8a22-41b16d66a972"))
                .Where("([umbracoNode].[id] = @0)", 1050)
                .Where("([cmsContentVersion].[VersionId] = @0)", new Guid("2b543516-a944-4ee6-88c6-8813da7aaa07"))
                .OrderBy("([cmsContentVersion].[VersionDate]) DESC");

            var sql = Sql();
            sql.SelectAll()
                .From<DocumentDto>()
                // fixme DocumentDto does not have VersionId anymore
                //.InnerJoin<ContentVersionDto>()
                //.On<DocumentDto, ContentVersionDto>(left => left.VersionId, right => right.VersionId)
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectType)
                .Where<NodeDto>(x => x.NodeId == 1050)
                .Where<ContentVersionDto>(x => x.VersionId == versionId)
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));
            Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
            for (int i = 0; i < expected.Arguments.Length; i++)
            {
                Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
            }

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_Verify_Property_Collection_Query()
        {
            var versionId = new Guid("2b543516-a944-4ee6-88c6-8813da7aaa07");
            var id = 1050;

            var expected = new Sql();
            expected.Select("*");
            expected.From("[" + Constants.DatabaseSchema.Tables.PropertyData + "]");
            expected.InnerJoin("[cmsPropertyType]").On("[" + Constants.DatabaseSchema.Tables.PropertyData + "].[propertytypeid] = [cmsPropertyType].[id]");
            expected.Where("([" + Constants.DatabaseSchema.Tables.PropertyData + "].[nodeId] = @0)", 1050);
            expected.Where("([" + Constants.DatabaseSchema.Tables.PropertyData + "].[versionId] = @0)", new Guid("2b543516-a944-4ee6-88c6-8813da7aaa07"));

            var sql = Sql();
            sql.SelectAll()
                .From<PropertyDataDto>()
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .Where<PropertyDataDto>(x => x.NodeId == id)
                .Where<PropertyDataDto>(x => x.VersionId == versionId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));
            Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
            for (int i = 0; i < expected.Arguments.Length; i++)
            {
                Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
            }

            Debug.Print(sql.SQL);
        }
    }
}
