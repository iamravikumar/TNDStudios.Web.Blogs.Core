﻿using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ser = Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Runtime.Serialization;
using TNDStudios.Blogs.Helpers;

namespace TNDStudios.Blogs.ViewModels
{
    /// <summary>
    /// Enumeration for the content parts for each of the display templates
    /// </summary>
    [DefaultValue(Unknown)]
    public enum BlogViewTemplatePart : Int32
    {
        Unknown = 0, // When the item cannot be found

        [EnumMember(Value = "indexheader")]
        Index_Header = 101,

        [EnumMember(Value = "indexbody")]
        Index_Body = 102,

        [EnumMember(Value = "indexitem")]
        Index_BlogItem = 103,

        [EnumMember(Value = "indexfooter")]
        Index_Footer = 104,

        [EnumMember(Value = "indexclearfix")]
        Index_Clearfix = 105,

        [EnumMember(Value = "indexclearfix-medium")]
        Index_Clearfix_Medium = 106,

        [EnumMember(Value = "indexclearfix-large")]
        Index_Clearfix_Large = 107,
    }

    /// <summary>
    /// Enumeration to identify the fields for each content part (actual field names in the description attribute)
    /// </summary>
    [DefaultValue(Unknown)]
    public enum BlogViewTemplateField : Int32
    {
        Unknown = 0, // When the item cannot be found

        [Description("items")]
        Index_Body_Items = 10201,

        [Description("clearfix")]
        Index_BlogItem_ClearFix = 10202,

        [Description("author")]
        Index_BlogItem_Author = 10301,

        [Description("description")]
        Index_BlogItem_Description = 10302,

        [Description("id")]
        Index_BlogItem_Id = 10303,

        [Description("name")]
        Index_BlogItem_Name = 10304,

        [Description("publisheddate")]
        Index_BlogItem_PublishedDate = 10305,

        [Description("state")]
        Index_BlogItem_State = 10306,

        [Description("updateddate")]
        Index_BlogItem_UpdatedDate = 10307,

    }

/// <summary>
/// Class to define the replacement text items
/// </summary>
public class BlogViewTemplateReplacement
    {
        public BlogViewTemplateField Id { get; set; }
        public String SearchString { get; set; }
        public String Content { get; set; }
        public Boolean Encode { get; set; }

        public BlogViewTemplateReplacement(BlogViewTemplateField id, String content, Boolean encode = true)
        {
            Id = id;
            SearchString = id.GetDescription();
            Content = content;
            Encode = encode;
        }

        public BlogViewTemplateReplacement(String searchString, String content, Boolean encode = true)
        {
            Id = BlogViewTemplateField.Unknown;
            SearchString = searchString;
            Content = content;
            Encode = encode;
        }
    }


    /// <summary>
    /// Serialisable class to load the blog display templates from Json
    /// </summary>
    [JsonObject]
    public class BlogViewTemplateLoader : BlogJsonBase
    {
        /// <summary>
        /// Array of the template items
        /// </summary>
        [JsonProperty(PropertyName = "Items", Required = Required.Always)]
        public List<BlogViewTemplateLoaderItem> Items { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public BlogViewTemplateLoader()
        {
            Items = new List<BlogViewTemplateLoaderItem>();
        }
    }

    /// <summary>
    /// Serialisable class for the individual template items to be loaded from Json
    /// </summary>
    [JsonObject]
    public class BlogViewTemplateLoaderItem : BlogJsonBase
    {
        /// <summary>
        /// Id for the template item (Uses a custom tolerant enum converter until Newtonsoft supports one)
        /// </summary>
        [JsonConverter(typeof(TolerantEnumConverter))]
        [JsonProperty(PropertyName = "Id", Required = Required.Always)]
        public BlogViewTemplatePart Id { get; set; }

        /// <summary>
        /// Content (template) of the template item
        /// </summary>
        [JsonProperty(PropertyName = "Content", Required = Required.Default)]
        public String Content { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public BlogViewTemplateLoaderItem()
        {
            Id = BlogViewTemplatePart.Unknown;
            Content = "";
        }
    }

    /// <summary>
    /// Handler to retrieve and load the templates from Json and index them
    /// </summary>
    public class BlogViewTemplates
    {
        /// <summary>
        /// Templates used to render the display
        /// </summary>
        private Dictionary<BlogViewTemplatePart, IHtmlContent> templates { get; set; }

        /// <summary>
        /// Get a HtmlTemplate from the dictionary with proper error trapping
        /// </summary>
        /// <returns></returns>
        public IHtmlContent Get(BlogViewTemplatePart key)
            => (templates.ContainsKey(key)) ? templates[key]
            : throw new HtmlTemplateNotFoundBlogException();
        
        /// <summary>
        /// Process / fill a template with a set of key value pairs
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public String Process(BlogViewTemplatePart key, List<BlogViewTemplateReplacement> values)
        {
            // Get the content (will raise an error if it fails)
            try
            {
                // Get the content
                IHtmlContent content = Get(key);

                // Something to process?
                String renderedContent = (content != null) ? HtmlHelpers.GetString(content) : "";
                if (renderedContent != "")
                {
                    // For each replacement text
                    values.ForEach(replacement => 
                    {
                        // Replace the value and check if it needs encoding for anti XSS or not
                        renderedContent = renderedContent.Replace("{" + replacement.SearchString + "}", 
                            replacement.Encode ? WebUtility.HtmlEncode(replacement.Content) : replacement.Content);
                    });
                }

                // Send the rendered content back
                return renderedContent;
            }
            catch (Exception ex)
            {
                throw BlogException.Passthrough(ex, new CastObjectBlogException(ex));
            }
        }

        /// <summary>
        /// Load the templates from a stream (which needs to be in the appropriate Json format)
        /// </summary>
        /// <param name="data">The loader object</param>
        /// <returns>Success Or Failure</returns>
        public Boolean Load(BlogViewTemplateLoader data)
        {
            // Must have something to work with so loop the items and load their content up
            data.Items.ForEach(item =>
            {
                // Add the content to the dictionary
                templates.Add(item.Id, new HtmlContentBuilder().Append(item.Content));
            });

            // Successful?
            return true;
        }
        
        /// <summary>
        /// Load the templates from a stream (which needs to be in the appropriate Json format)
        /// </summary>
        /// <param name="stream">A stream object that contains the loader object</param>
        /// <returns>Success Or Failure</returns>
        public Boolean Load(Stream stream)
        {
            BlogViewTemplateLoader templateLoader = null; // Define the template loader outside the try

            try
            {
                templates = new Dictionary<BlogViewTemplatePart, IHtmlContent>(); // Empty list of templates by default

                // Convert stream to string
                using (StreamReader reader = new StreamReader(stream))
                {
                    String serialisedObject = reader.ReadToEnd();

                    // Custom event handler for this flavour of deserialisation
                    EventHandler<ser.ErrorEventArgs> internalErrorHandler = delegate (object sender, ser.ErrorEventArgs args)
                    {
                        // Check the member type
                        /*
                        switch (args.ErrorContext.Member)
                        {
                            // Issue converting the Id (as they mistyped the enum value) -- Now replaced with TolerentEnumConverter
                            case "Id":
                                
                                break;
                        }
                        */

                        args.ErrorContext.Handled = false; // true
                    };

                    // Deserialise the string to the template loader format
                    templateLoader = JsonConvert.DeserializeObject<BlogViewTemplateLoader>(serialisedObject, new JsonSerializerSettings()
                    {
                        Error = internalErrorHandler
                    });

                    // Nothing to work with? Raise the error
                    if (templateLoader == null || templateLoader.Items == null || templateLoader.Items.Count == 0)
                        throw new CastObjectBlogException();
                }

            }
            catch (Exception ex)
            {
                // Decide whether or not to pass the origional or inner exception to the user
                throw BlogException.Passthrough(ex, new HtmlTemplateLoadFailureBlogException(ex));
            }

            // Call the loader with the object and not the stream
            return Load(templateLoader);
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public BlogViewTemplates()
        {
            templates = new Dictionary<BlogViewTemplatePart, IHtmlContent>(); // Empty list of templates by default
        }

    }
}
