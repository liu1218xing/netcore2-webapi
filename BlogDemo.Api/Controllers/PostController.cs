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
using Microsoft.Extensions.Configuration;
using AutoMapper;
using BlogDemo.Infrastructure.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using BlogDemo.Infrastructure.Extensions;
using BlogDemo.Infrastructure.Services;

namespace BlogDemo.Api.Controllers
{
    [Route("api/posts")]
    public class PostController: Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly ITypeHelperService _typeHelperService;
        private readonly IPropertyMappingContainer _propertyMappingContainer;
        private readonly IMapper _mapper;

        //private readonly ILogger<PostController> _logger;

        public PostController(IPostRepository postRepository,
            IUnitOfWork unitOfWork,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IUrlHelper urlHelper,
            ITypeHelperService typeHelperService,
            IPropertyMappingContainer propertyMappingContainer
            //ILogger<PostController>  logger
            )
        {
            _postRepository = postRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _mapper = mapper;
            _logger= loggerFactory.CreateLogger("BlogDemo.Api.Controllers");
            _urlHelper  = urlHelper;
            _typeHelperService = typeHelperService;
            _propertyMappingContainer = propertyMappingContainer;
        }

        [HttpGet(Name = "GetPosts")]
        public async Task<IActionResult> Get(PostParameters postParameters)
        {
            if (!_propertyMappingContainer.ValidateMappingExistsFor<PostResource, Post>(postParameters.OrderBy))
            {
                return BadRequest("Can't finds fields for sorting.");
            }

            if (!_typeHelperService.TypeHasProperties<PostResource>(postParameters.Fields))
            {
                return BadRequest("Fields not exist.");
            }
            var postList = await _postRepository.GetAllPostsAsyn(postParameters);
            //_logger.LogInformation("Get All Posts...");
            var v = _configuration["Key1"];
            _logger.LogError("Get All Posts...");
            var postRescources = _mapper.Map<IEnumerable<Post>, IEnumerable<PostResource>>(postList);
            var result = postRescources.ToDynamicIEnumerable(postParameters.Fields);
            var previousPageLink = postList.HasPrevious ?
                CreatePostUri(postParameters, PaginationResourceUriType.PreviousPage) : null;

            var nextPageLink = postList.HasNext ?
                CreatePostUri(postParameters, PaginationResourceUriType.NextPage) : null;
            var meta = new
            {
                 postList.PageSize,
                 postList.PageIndex,
                 postList.TotalItemsCount,
                 postList.PageCount,
                previousPageLink,
                nextPageLink
            };
            Response.Headers.Add("x-pagination",JsonConvert.SerializeObject(meta,new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
            //throw new Exception("Error!!!!!");
            return Ok(result);
        }

        [HttpGet("{id}")]   
        public async Task<IActionResult> Get(int id,string fields = null)
        {
            if (!_typeHelperService.TypeHasProperties<PostResource>(fields))
            {
                return BadRequest("Fields not exist.");
            }
            var post= await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            var postResource = _mapper.Map<Post, PostResource>(post);
            var result = postResource.ToDynamic(fields);
            return Ok(result);
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
        private string CreatePostUri(PostParameters parameters, PaginationResourceUriType uriType)
        {
            switch (uriType)
            {
                case PaginationResourceUriType.PreviousPage:
                    var previousParameters = new
                    {
                        pageIndex = parameters.PageIndex - 1,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    };
                    return _urlHelper.Link("GetPosts", previousParameters);
                case PaginationResourceUriType.NextPage:
                    var nextParameters = new
                    {
                        pageIndex = parameters.PageIndex + 1,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    };
                    return _urlHelper.Link("GetPosts", nextParameters);
                default:
                    var currentParameters = new
                    {
                        pageIndex = parameters.PageIndex,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    };
                    return _urlHelper.Link("GetPosts", currentParameters);
            }
        }

        private IEnumerable<LinkResource> CreateLinksForPost(int id, string fields = null)
        {
            var links = new List<LinkResource>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkResource(
                        _urlHelper.Link("GetPost", new { id }), "self", "GET"));
            }
            else
            {
                links.Add(
                    new LinkResource(
                        _urlHelper.Link("GetPost", new { id, fields }), "self", "GET"));
            }

            links.Add(
                new LinkResource(
                    _urlHelper.Link("DeletePost", new { id }), "delete_post", "DELETE"));

            return links;
        }

    }
}
