using BlogDemo.Core.Entities;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlogDemo.Infrastructure.Repositories
{
    public class PostRepository: IPostRepository
    {
        private readonly MyContext _myContext;

        public PostRepository(MyContext myContext)
        {
            _myContext = myContext;
        }

        public void AddPost(Post post)
        {
            _myContext.Add(post);
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsyn()
        {
            return await _myContext.Posts.ToListAsync();
             
        }

    }
}
