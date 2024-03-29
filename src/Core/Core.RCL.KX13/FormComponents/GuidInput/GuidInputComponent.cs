﻿using Kentico.Forms.Web.Mvc;

namespace Core.RCL.KX13.Models.FormComponents.GuidInput
{
    /// <summary>
    /// Basic Guid form component
    /// </summary>
    public class GuidInputComponent : FormComponent<GuidInputComponentProperties, Guid>
    {
        public const string IDENTIFIER = "GuidInputFormComponent";

        [BindableProperty]
        public string Value { 
            get {
                return GuidValue != default ? GuidValue.ToString() : "";
            } set {
                if(!string.IsNullOrWhiteSpace(value))
                {
                    GuidValue = new Guid(value);
                }
            } 
        }

        public Guid GuidValue { get; set; }

        // Gets the value of the form field instance passed from a view where the instance is rendered
        public override Guid GetValue()
        {
            return GuidValue;
        }

        public override void SetValue(Guid value)
        {
            GuidValue = value;
        }
    }
}