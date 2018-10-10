using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogDemo.Core.Entities;
using BlogDemo.Infrastructure.Resources;

namespace BlogDemo.Api.Extensions
{
    public class MappingProFile:Profile
    {
        public MappingProFile()
        {
            CreateMap<Post, PostResource>()
                .ForMember(dest =>dest.UpdateTime,opt=>opt.MapFrom(src=>src.LastModified));
            CreateMap<PostResource,Post>();
            CreateMap<PostAddResource, Post>();
            CreateMap<PostUpdateResource, Post>();
        }
    }
}
