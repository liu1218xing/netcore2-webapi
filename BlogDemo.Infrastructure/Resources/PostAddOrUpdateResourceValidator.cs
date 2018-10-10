using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDemo.Infrastructure.Resources
{
    public class PostAddOrUpdateResourceValidator<T>: AbstractValidator<T> where T:PostAddOrUpdateResource
    {
        public PostAddOrUpdateResourceValidator()
        {
            RuleFor(x => x.Title)
                .NotNull()
                .WithName("标题")
                .WithMessage("required|{PropertyName}是必填项")
                .MaximumLength(50)
                .WithMessage("maxlength|{PropertyName}最大长度为{MaxLength}");
            RuleFor(x => x.Body)
                .NotNull()
                .WithName("正文")
                .WithMessage("required|{PropertyName}是必填项")
                .MinimumLength(100)
                .WithMessage("MinLength|{PropertyName}最小长度为{MinLength}");
        }
    }
}
