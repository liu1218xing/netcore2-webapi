using BlogDemo.Core.Entities;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.DataBase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDemo.Api.Controllers
{
    [Route("api/posts")]
    public class PostController: Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        //private readonly ILogger<PostController> _logger;

        public PostController(IPostRepository postRepository,
            IUnitOfWork unitOfWork,
            ILoggerFactory loggerFactory
            //ILogger<PostController>  logger
            )
        {
            _postRepository = postRepository;
            _unitOfWork = unitOfWork;
            _logger= loggerFactory.CreateLogger("BlogDemo.Api.Controllers");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var posts = await _postRepository.GetAllPostsAsyn();
            //_logger.LogInformation("Get All Posts...");
            _logger.LogError("Get All Posts...");
            return Ok(posts);
        }
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var newPost = new Post
            {
                Author = "Admin",
                Body = "1231321312312321312321321",
                Title = "Title A",
                LastModified = DateTime.Now
            };
            _postRepository.AddPost(newPost);
            await _unitOfWork.SaveAsync();
            return Ok();
        }
    }
}
