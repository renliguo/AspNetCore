// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Microsoft.AspNetCore.Components.Forms
{
    /// <summary>
    /// Displays a list of validation messages for a specified field within a cascaded <see cref="EditContext"/>.
    /// </summary>
    public class ValidationMessage<T> : ComponentBase, IDisposable
    {
        private EditContext _previousEditContext;
        private Expression<Func<T>> _previousFieldAccessor;
        private readonly EventHandler<ValidationStateChangedEventArgs> _validationStateChangedHandler;
        private FieldIdentifier _fieldIdentifier;

        [CascadingParameter] EditContext CurrentEditContext { get; set; }

        /// <summary>
        /// Specifies the field for which validation messages should be displayed.
        /// </summary>
        [Parameter] public Expression<Func<T>> For { get; private set; }

        /// <summary>`
        /// Constructs an instance of <see cref="ValidationSummary"/>.
        /// </summary>
        public ValidationMessage()
        {
            _validationStateChangedHandler = (sender, eventArgs) => StateHasChanged();
        }

        /// <inheritdoc />
        protected override void OnParametersSet()
        {
            if (CurrentEditContext == null)
            {
                throw new InvalidOperationException($"{GetType()} requires a cascading parameter " +
                    $"of type {nameof(EditContext)}. For example, you can use {GetType()} inside " +
                    $"an {nameof(EditForm)}.");
            }

            if (For == null) // Not possible except if you manually specify T
            {
                throw new InvalidOperationException($"{GetType()} requires a value for the " +
                    $"{nameof(For)} parameter.");
            }
            else if (For != _previousFieldAccessor)
            {
                _fieldIdentifier = FieldIdentifier.Create(For);
                _previousFieldAccessor = For;
            }

            if (CurrentEditContext != _previousEditContext)
            {
                DetachValidationStateChangedListener();
                CurrentEditContext.OnValidationStateChanged += _validationStateChangedHandler;
                _previousEditContext = CurrentEditContext;
            }
        }

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            foreach (var message in CurrentEditContext.GetValidationMessages(_fieldIdentifier))
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "validation-message");
                builder.AddContent(2, message);
                builder.CloseElement();
            }
        }

        private void HandleValidationStateChanged(object sender, ValidationStateChangedEventArgs eventArgs)
        {
            StateHasChanged();
        }

        void IDisposable.Dispose()
        {
            DetachValidationStateChangedListener();
        }

        private void DetachValidationStateChangedListener()
        {
            if (_previousEditContext != null)
            {
                _previousEditContext.OnValidationStateChanged -= _validationStateChangedHandler;
            }
        }
    }
}
