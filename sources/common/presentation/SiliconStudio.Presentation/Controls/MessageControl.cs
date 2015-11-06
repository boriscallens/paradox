﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SiliconStudio.Presentation.Controls
{
    [TemplatePart(Name = MessageContainerPartName, Type = typeof(FlowDocumentScrollViewer))]
    public class MessageControl : ContentControl
    {
        /// <summary>
        /// The name of the part for the <see cref="FlowDocumentScrollViewer"/> container.
        /// </summary>
        public const string MessageContainerPartName = "PART_MessageContainer";

        /// <summary>
        /// The container in which the message is displayed.
        /// </summary>
        private FlowDocumentScrollViewer messageContainer;

        static MessageControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MessageControl), new FrameworkPropertyMetadata(typeof(MessageControl)));
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            messageContainer = GetTemplateChild(MessageContainerPartName) as FlowDocumentScrollViewer;
            if (messageContainer == null)
                throw new InvalidOperationException($"A part named '{MessageContainerPartName}' must be present in the ControlTemplate, and must be of type '{typeof(FlowDocumentScrollViewer)}'.");

            ResetMessage();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            ResetMessage();
        }

        private void ResetMessage()
        {
            if (messageContainer != null)
            {
                var paragraph = ProcessMessage();
                messageContainer.Document = new FlowDocument(paragraph)
                {
                    IsHyphenationEnabled = true,
                    // better layout rendering, but use more CPU
                    IsOptimalParagraphEnabled = true,
                };
            }
        }

        private Paragraph ProcessMessage()
        {
            var paragraph = new Paragraph();
            // TODO: support mardown text
            paragraph.Inlines.Add(Content?.ToString() ?? "Nothing to display");
            return paragraph;
        }
    }
}
