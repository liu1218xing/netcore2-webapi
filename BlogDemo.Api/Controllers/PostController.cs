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
using BlogDemo.Api.Helpers;
using Microsoft.AspNetCore.JsonPatch;

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
        [RequestHeaderMatchingMediaType("Accept", new[] { "application/vnd.cgzl.hateoas+json" })]
        public async Task<IActionResult> GetHateoas(PostParameters postParameters)
        {
            if (!_propertyMappingContainer.ValidateMappingExistsFor<PostResource, Post>(postParameters.OrderBy))
            {
                return BadRequest("Can't finds fields for sorting.");
            }

            if (!_typeHelperService.TypeHasProperties<PostResource>(postParameters.Fields))
            {
                return BadRequest("Fields not exist.");
            }

            var postList = await _postRepository.GetAllPostsAsync(postParameters);
            var postResources = _mapper.Map<IEnumerable<Post>, IEnumerable<PostResource>>(postList);

            var shapedPostResources = postResources.ToDynamicIEnumerable(postParameters.Fields);
            var shapedWithLinks = shapedPostResources.Select(x =>
            {
                var dict = x as IDictionary<string, object>;
                var postLinks = CreateLinksForPost((int)dict["Id"], postParameters.Fields);
                dict.Add("links", postLinks);
                return dict;
            });
            var links = CreateLinksForPosts(postParameters, postList.HasPrevious, postList.HasNext);
            var result = new
            {
                value = shapedWithLinks,
                links
            };

            var meta = new
            {
                postList.PageSize,
                postList.PageIndex,
                postList.TotalItemsCount,
                postList.PageCount
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(meta, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

            return Ok(result);
        }

        [HttpGet(Name = "GetPosts")]
        [RequestHeaderMatchingMediaType("Accept", new[] { "application/json" })]
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

            var postList = await _postRepository.GetAllPostsAsync(postParameters);

            var postResources = _mapper.Map<IEnumerable<Post>, IEnumerable<PostResource>>(postList);

            var previousPageLink = postList.HasPrevious ?
                CreatePostUri(postParameters,
                    PaginationResourceUriType.PreviousPage) : null;

            var nextPageLink = postList.HasNext ?
                CreatePostUri(postParameters,
                    PaginationResourceUriType.NextPage) : null;

            var meta = new
            {
                postList.TotalItemsCount,
                postList.PageSize,
                postList.PageIndex,
                postList.PageCount,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(meta, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

            return Ok(postResources.ToDynamicIEnumerable(postParameters.Fields));
        }

        //[HttpGet(Name = "GetPosts")]
        //public async Task<IActionResult> Get(PostParameters postParameters)
        //{
        //    if (!_propertyMappingContainer.ValidateMappingExistsFor<PostResource, Post>(postParameters.OrderBy))
        //    {
        //        return BadRequest("Can't finds fields for sorting.");
        //    }

        //    if (!_typeHelperService.TypeHasProperties<PostResource>(postParameters.Fields))
        //    {
        //        return BadRequest("Fields not exist.");
        //    }
        //    var postList = await _postRepository.GetAllPostsAsyn(postParameters);
        //    //_logger.LogInformation("Get All Posts...");
        //    var v = _configuration["Key1"];
        //    _logger.LogError("Get All Posts...");
        //    var postResources = _mapper.Map<IEnumerable<Post>, IEnumerable<PostResource>>(postList);

        //    var shapedPostResources = postResources.ToDynamicIEnumerable(postParameters.Fields);
        //    var shapedWithLinks = shapedPostResources.Select(x =>
        //    {
        //        var dict = x as IDictionary<string, object>;
        //        var postLinks = CreateLinksForPost((int)dict["Id"], postParameters.Fields);
        //         dict.Add("links", postLinks);
        //         return dict;

        //     });
        //    var links = CreateLinksForPosts(postParameters, postList.HasPrevious, postList.HasNext);
        //    var result = new
        //    {
        //        value = shapedWithLinks,
        //        links
        //    };
        //    var previousPageLink = postList.HasPrevious ?
        //        CreatePostUri(postParameters, PaginationResourceUriType.PreviousPage) : null;

        //    var nextPageLink = postList.HasNext ?
        //        CreatePostUri(postParameters, PaginationResourceUriType.NextPage) : null;
        //    var meta = new
        //    {
        //         postList.PageSize,
        //         postList.PageIndex,
        //         postList.TotalItemsCount,
        //         postList.PageCount,
        //        previousPageLink,
        //        nextPageLink
        //    };
        //    Response.Headers.Add("x-pagination",JsonConvert.SerializeObject(meta,new JsonSerializerSettings
        //    {
        //        ContractResolver = new CamelCasePropertyNamesContractResolver()
        //    }));
        //    //throw new Exception("Error!!!!!");
        //    return Ok(result);
        //}

        [HttpGet("{id}",Name= "GetPost")]   
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
            var shapedPostResources = postResource.ToDynamic(fields);
            var links = CreateLinksForPost(id, fields);

            var result = shapedPostResources as IDictionary<string, object>;
            result.Add("links", links);
            return Ok(result);
        }
        [HttpPost(Name = "CreatePost")]
        [RequestHeaderMatchingMediaType("Content-Type", new[] { "application/vnd.cgzl.post.create+json" })]
        [RequestHeaderMatchingMediaType("Accept", new[] { "application/vnd.cgzl.hateoas+json" })]
        public async Task<IActionResult> Post([FromBody] PostAddResource postAddResource)
        {
            if(postAddResource== null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return new MyUnprocessableEntityObjectResult(ModelState);
            }
            var newPost = _mapper.Map<PostAddResource, Post>(postAddResource);
            newPost.Author = "admin";
            newPost.LastModified = DateTime.Now;

            _postRepository.AddPost(newPost);
            if(! await _unitOfWork.SaveAsync())
            {
                throw new Exception("Save Failed!");
            }
            var resultResource = _mapper.Map<Post, PostResource>(newPost);
            var links = CreateLinksForPost(newPost.Id);
            var linkedPostResource = resultResource.ToDynamic() as IDictionary<string, object>;
            linkedPostResource.Add("links", links);

            return CreatedAtRoute("GetPost",new { id = linkedPostResource["Id"]},linkedPostResource );
        }
        [HttpDelete("{id}",Name = "DeletePost")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            _postRepository.Delete(post);
            if( ! await _unitOfWork.SaveAsync())
            {
                throw new Exception($"Deleting post {id} failed when saving.");
            }
            return NoContent();
        }
        [HttpPut("{id}",Name = "UpdatePost")]
        [RequestHeaderMatchingMediaType("Content-Type", new[] { "application/vnd.cgzl.post.update+json" })]
        public async Task<IActionResult> UpdatePost(int id,[FromBody] PostUpdateResource postUpdate)
        {
            if (postUpdate == null)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                return new MyUnprocessableEntityObjectResult(ModelState);
            }
            var post =await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            post.LastModified = DateTime.Now;
            _mapper.Map(postUpdate, post);
            if(! await _unitOfWork.SaveAsync())
            {
                throw new Exception($"Updating post {id} failed when saving.");
            }
            return NoContent();

        }
        //[HttpPatch("{id}", Name = "PartiallyUpdatePost")]
        //public async Task<IActionResult> PartiallyUpdateCityForCountry(int id,
        //    [FromBody] JsonPatchDocument<PostUpdateResource> patchDoc)
        //{
        //    if(patchDoc == null)
        //    {
        //        return BadRequest();
        //    }
        //    var post = await _postRepository.GetPostByIdAsync(id);
        //    if(post == null)
        //    {
        //        return NotFound();
        //    }
        //    var postToPatch = _mapper.Map<PostUpdateResource>(post);
        //    patchDoc.ApplyTo(postToPatch, ModelState);
        //    TryValidateModel(postToPatch);
        //    if (!ModelState.IsValid)
        //    {
        //        return new MyUnprocessableEntityObjectResult(ModelState);
        //    }

        //    _mapper.Map(postToPatch, post);
        //    post.LastModified = DateTime.Now;
        //    _postRepository.Update(post);

        //    if (!await _unitOfWork.SaveAsync())
        //    {
        //        throw new Exception($"Patching city {id} failed when saving.");
        //    }

        //    return NoContent();

        //}

        [HttpPatch("{id}", Name = "PartiallyUpdatePost")]
        public async Task<IActionResult> PartiallyUpdateCityForCountry(int id,
            [FromBody] JsonPatchDocument<PostUpdateResource> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var postToPatch = _mapper.Map<PostUpdateResource>(post);

            patchDoc.ApplyTo(postToPatch, ModelState);

            TryValidateModel(postToPatch);

            if (!ModelState.IsValid)
            {
                return new MyUnprocessableEntityObjectResult(ModelState);
            }

            _mapper.Map(postToPatch, post);
            post.LastModified = DateTime.Now;
            _postRepository.Update(post);

            if (!await _unitOfWork.SaveAsync())
            {
                throw new Exception($"Patching city {id} failed when saving.");
            }

            return NoContent();
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
        private IEnumerable<LinkResource> CreateLinksForPosts(PostParameters postResourceParameters,
            bool hasPrevious, bool hasNext)
        {
            var links = new List<LinkResource>
            {
                new LinkResource(
                    CreatePostUri(postResourceParameters, PaginationResourceUriType.CurrentPage),
                    "self", "GET")
            };

            if (hasPrevious)
            {
                links.Add(
                    new LinkResource(
                        CreatePostUri(postResourceParameters, PaginationResourceUriType.PreviousPage),
                        "previous_page", "GET"));
            }

            if (hasNext)
            {
                links.Add(
                    new LinkResource(
                        CreatePostUri(postResourceParameters, PaginationResourceUriType.NextPage),
                        "next_page", "GET"));
            }

            return links;
        }

    }
}
