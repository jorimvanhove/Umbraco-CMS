﻿using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MemberFactory
    {
        /// <summary>
        /// Builds an IMedia item from a dto and content type.
        /// </summary>
        public static Member BuildEntity(MemberDto dto, IMemberType contentType)
        {
            var nodeDto = dto.ContentDto.NodeDto;
            var contentVersionDto = dto.ContentVersionDto;

            var content = new Member(nodeDto.Text, dto.Email, dto.LoginName, dto.Password, contentType);

            try
            {
                content.DisableChangeTracking();

                content.Id = dto.NodeId;
                content.Key = nodeDto.UniqueId;
                content.Version = contentVersionDto.VersionId;

                // fixme missing names?

                content.Path = nodeDto.Path;
                content.Level = nodeDto.Level;
                content.ParentId = nodeDto.ParentId;
                content.SortOrder = nodeDto.SortOrder;
                content.Trashed = nodeDto.Trashed;

                content.CreatorId = nodeDto.UserId ?? 0;
                // fixme missing writerId - which then should move to nodeDto
                content.CreateDate = nodeDto.CreateDate;
                content.UpdateDate = contentVersionDto.VersionDate;

                content.ProviderUserKey = content.Key; // fixme explain

                // reset dirty initial properties (U4-1946)
                content.ResetDirtyProperties(false);
                return content;
            }
            finally
            {
                content.EnableChangeTracking();
            }
        }

        /// <summary>
        /// Buils a dto from an IMember item.
        /// </summary>
        public static MemberDto BuildDto(IMember entity)
        {
            var contentDto = BuildContentDto(entity);

            var dto = new MemberDto
            {
                Email = entity.Email,
                LoginName = entity.Username,
                NodeId = entity.Id,
                Password = entity.RawPasswordValue,

                ContentDto = contentDto,
                ContentVersionDto = BuildContentVersionDto(entity, contentDto)
            };
            return dto;
        }

        private static ContentDto BuildContentDto(IMember entity)
        {
            var dto = new ContentDto
            {
                // Id = _primaryKey if >0 - fixme - kill that id entirely
                NodeId = entity.Id,
                ContentTypeId = entity.ContentTypeId,

                NodeDto = BuildNodeDto(entity)
            };

            return dto;
        }

        private static NodeDto BuildNodeDto(IMember entity)
        {
            var dto = new NodeDto
            {
                NodeId = entity.Id,
                UniqueId = entity.Key,
                ParentId = entity.ParentId,
                Level = Convert.ToInt16(entity.Level),
                Path = entity.Path,
                SortOrder = entity.SortOrder,
                Trashed = entity.Trashed,
                UserId = entity.CreatorId,
                Text = entity.Name,
                NodeObjectType = Constants.ObjectTypes.Member,
                CreateDate = entity.CreateDate
            };

            return dto;
        }

        private static ContentVersionDto BuildContentVersionDto(IMember entity, ContentDto contentDto)
        {
            var dto = new ContentVersionDto
            {
                //Id =, // fixme
                NodeId = entity.Id,
                VersionId = entity.Version,
                VersionDate = entity.UpdateDate,
                Current = true, // always building the current one
                Text = entity.Name,

                ContentDto = contentDto
            };

            return dto;
        }
    }
}
