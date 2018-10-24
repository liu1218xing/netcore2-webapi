﻿using BlogDemo.Core.Entities;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.DataBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDemo.Infrastructure.Repositories
{
    public class PostImageRepository : IPostImageRepository
    {
        private readonly MyContext _myContext;

        public PostImageRepository(MyContext myContext)
        {
            _myContext = myContext;
        }

        public void Add(PostImage postImage)
        {
            _myContext.PostImages.Add(postImage);
        }

    }
}
