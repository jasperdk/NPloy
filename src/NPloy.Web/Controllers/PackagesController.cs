using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using NuGet.Server.DataServices;

namespace NPloy.Web.Controllers
{
//    [JsonLast]
    public class PackagesController : ODataController
    {
        private readonly Packages _packages;
        //private IServerPackageRepository _repository;

            public PackagesController()
        {
            _packages = new Packages();
                //_repository= new ServerPackageRepository(@"d:\temp\packages");
        }

        public IQueryable<Package> Get()
        {
            //var packages = from p in _repository.GetPackages()
            //               select _repository.GetMetadataPackage(p);
            //return packages.InterceptWith(new PackageIdComparisonVisitor());
            return _packages.Search("", "", true);
        }

        [HttpGet]
        [System.Web.Http.Queryable]
        public IQueryable<Package> Search(string searchTerm = "", string targetFramework = "", bool includePrerelease = false)
        {
            var packages = _packages.Search(searchTerm, targetFramework, includePrerelease);
            return packages;
        }

        [HttpGet]
        [System.Web.Http.Queryable]
        public IQueryable<Package> FindPackagesById(string id)
        {
            var packages = _packages.FindPackagesById(id);
            return packages;
        }

        [HttpGet]
        [System.Web.Http.Queryable]
        public IQueryable<Package> GetUpdates(
            string packageIds, string versions, bool includePrerelease, bool includeAllVersions, string targetFrameworks,
            string versionConstraints)
        {
            return _packages.GetUpdates(packageIds, versions, includePrerelease, includeAllVersions, targetFrameworks,
                                 versionConstraints);
        }
    }

    public class JsonLastAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            MediaTypeFormatterCollection formatters = controllerDescriptor.Configuration.Formatters;

            // Create a copy of the list of all JSON formatters.
            IEnumerable<MediaTypeFormatter> jsonFormatters = formatters.Where(f => f.SupportedMediaTypes.Any(
                m => m.MediaType == "application/json")).ToList();

            // Remove them from their current positions in the list.
            foreach (MediaTypeFormatter jsonFormatter in jsonFormatters)
            {
                formatters.Remove(jsonFormatter);
            }

            // Add them back to the end of the list.
            foreach (MediaTypeFormatter jsonFormatter in jsonFormatters)
            {
                formatters.Add(jsonFormatter);
            }
        }
    }
}