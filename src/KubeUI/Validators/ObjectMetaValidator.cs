using FluentValidation;
using KubeUI.Schema;
using KubeUI.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace KubeUI.Validators
{
    public class ObjectMetaValidator : AbstractValidator<ObjectMeta>
    {
        public ObjectMetaValidator(IServiceProvider service)
        {
            var state = service.GetRequiredService<IState>();

            //RuleFor(x => x.Name).NotEmpty();

            //RuleFor(x => x.Name)
            //    .Must(_ => !state.GetCollection(typeof(Deployment))
            //    .GroupBy(y => new { name = string.IsNullOrEmpty(((Deployment)y).Metadata.Name) ? null : ((Deployment)y).Metadata.Name, ((Deployment)y).Metadata.Namespace })
            //    .Any(y => y.Count() > 1))
            //    .WithMessage("Name must be unique in this namespace.");
        }
    }
}
