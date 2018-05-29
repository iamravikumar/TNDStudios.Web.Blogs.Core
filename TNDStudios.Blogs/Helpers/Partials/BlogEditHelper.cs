﻿
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using TNDStudios.Blogs.ViewModels;

namespace TNDStudios.Blogs.Helpers
{
    public static partial class HtmlHelpers
    {
        /// <summary>
        /// Wrapper for the underlaying renderer for the blog item editor
        /// </summary>
        /// <param name="helper">The HtmlHelper reference to extend the function in to</param>
        /// <param name="item">The item to be rendered</param>
        /// <param name="preview">Display in "prevew" mode for indexes etc.</param>
        /// <returns>The Html String output for the helper</returns>
        public static IHtmlContent BlogEditItem(this IHtmlHelper helper, IBlogItem item)
            => BlogEditItem(item, GetModel(helper));
        
        /// <summary>
        /// None-helper signature of the blog item blog item editor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="preview"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public static IHtmlContent BlogEditItem(IBlogItem item, BlogViewModelBase viewModel)
            => ContentFill(BlogViewTemplatePart.Blog_EditItem,
                new List<BlogViewTemplateReplacement>()
                {
                    new BlogViewTemplateReplacement(BlogViewTemplateField.Common_Controller_Url, viewModel.ControllerUrl, false),
                    new BlogViewTemplateReplacement(BlogViewTemplateField.BlogItem_Author, item.Header.Author, true),
                    new BlogViewTemplateReplacement(BlogViewTemplateField.BlogItem_Description, item.Header.Description, true),
                    new BlogViewTemplateReplacement(BlogViewTemplateField.BlogItem_Id, item.Header.Id, true),
                    new BlogViewTemplateReplacement(BlogViewTemplateField.BlogItem_Name, item.Header.Name, true),
                    new BlogViewTemplateReplacement(BlogViewTemplateField.BlogItem_PublishedDate,
                        item.Header.PublishedDate.ToCustomDate(viewModel.DisplaySettings.DateFormat), true),
                    new BlogViewTemplateReplacement(BlogViewTemplateField.BlogItem_State, item.Header.State.GetDescription(), true),
                    new BlogViewTemplateReplacement(BlogViewTemplateField.BlogItem_UpdatedDate,
                        item.Header.UpdatedDate.ToCustomDate(viewModel.DisplaySettings.DateFormat), true),
                    new BlogViewTemplateReplacement(BlogViewTemplateField.BlogItem_Content, item.Content, false),
                    new BlogViewTemplateReplacement(BlogViewTemplateField.BlogItem_SEOUrlTitle, SEOUrlTitle(item.Header.Name), false),
                    new BlogViewTemplateReplacement(BlogViewTemplateField.Attachments, EditAttachments(item, viewModel).GetString(), false)

                }, viewModel);

        /// <summary>
        /// Build the attachment editing content
        /// </summary>
        /// <param name="item">The blog entry to get the attachments from</param>
        /// <param name="viewModel">The view model that contains the relevant templates</param>
        /// <returns>The Html Content of the attachment editor</returns>
        private static IHtmlContent EditAttachments(IBlogItem item, BlogViewModelBase viewModel)
        {
            // Create a content builder just to make the looped items content
            HtmlContentBuilder attachmentBuilder = new HtmlContentBuilder();

            // Build the clearfix insert classes from the model templates so we don't have to retrieve them each time
            //String clearFixMedium = String.Format(" {0}", viewModel.Templates.Get(BlogViewTemplatePart.Index_Clearfix_Medium).GetString());
            //String clearFixLarge = String.Format(" {0}", viewModel.Templates.Get(BlogViewTemplatePart.Index_Clearfix_Large).GetString());

            // Loop the results and create the row for each result in the itemsBuilder
            item.Files.ForEach(
                file => {
                    attachmentBuilder.AppendHtml(EditAttachment(file, viewModel));
                }
                );

            // Call the standard content filler function
            return ContentFill(BlogViewTemplatePart.Attachments,
                new List<BlogViewTemplateReplacement>()
                {
                    new BlogViewTemplateReplacement(BlogViewTemplateField.Attachment_List, attachmentBuilder.GetString(), false)
                },
                viewModel);
        }

        /// <summary>
        /// Build the singular attachment editor
        /// </summary>
        /// <param name="file">The file (attachment) to be displayed</param>
        /// <param name="viewModel">The view model that contains the relevant templates</param>
        /// <returns>The Html Content for the attachment editor</returns>
        private static IHtmlContent EditAttachment(BlogFile file, BlogViewModelBase viewModel)
            => ContentFill(BlogViewTemplatePart.Attachment_Item,
                new List<BlogViewTemplateReplacement>()
                {
                    new BlogViewTemplateReplacement(BlogViewTemplateField.Common_Controller_Url, viewModel.ControllerUrl, false)
                }, viewModel);
    }
}