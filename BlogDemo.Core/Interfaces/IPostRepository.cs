﻿using BlogDemo.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlogDemo.Core.Interfaces
{
    public interface  IPostRepository
    {
        Task<IEnumerable<Post>> GetAllPostsAsyn();
        void AddPost(Post post);
    }
}
