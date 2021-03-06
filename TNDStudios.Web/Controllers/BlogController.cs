﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;
using TNDStudios.Web.Blogs.Core.Attributes;
using TNDStudios.Web.Blogs.Core.Controllers;

namespace TNDStudios.Web.Controllers
{
    [BlogSEO(author:"The Naked Developer", title: "Blogging Platform Test", description: "Description of the controller level", tags: "Some, Keywords, To check")]
    [BlogSetup(blogId: "testBlog", provider: "TNDStudios.Web.Blogs.Core.Providers.BlogXmlProvider", providerConnectionString: "path=..\\AppData")]
    public class BlogController : BlogControllerBase
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public BlogController() : base() //ICompositeViewEngine viewEngine) : base(viewEngine)
        {
        }

        /// <summary>
        /// Override the startup method for when the blog is first run
        /// </summary>
        public override void BlogInitialised()
        {   
            /*
#warning [Temporary Code, Remove once tested]
            String content = $"Lorem ipsum dolor sit amet, magna augue te mei. Suas tantas cu eam, vel quas ignota at, ei oblique epicuri aliquando sed. Te alienum mentitum nec. Option propriae menandri an quo, vidisse delicatissimi ut qui, id per amet utroque. No pri decore libris adversarium. Ut has vero labores temporibus, at vel quis accusata, te ullum iudicabit mel.";

            for (var blogID = 0; blogID < 100; blogID++)
                base.Currrent.Save(
                    new BlogItem()
                    {
                        Header = new BlogHeader()
                        {
                            Author = "Joe Walters",
                            Name = "New Blog (" + blogID.ToString() + ")",
                            Description = content.Substring(0, (int)((new Random()).NextDouble() * 255.0)),
                            State = BlogHeaderState.Published,
                            PublishedDate = DateTime.Now
                        },
                        Content = content
                    });
                    */
            // Call the base implementation
            base.BlogInitialised();
        }
    }
}