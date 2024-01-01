using Kentico.Forms.Web.Mvc;

namespace Core.RCL.KX13.Models.FormComponents.DoubleInput
{
    public class DoubleInputComponent : FormComponent<DoubleInputComponentProperties, double>
    {
        public const string IDENTIFIER = "DoubleInputFormComponent";
        
        
        [BindableProperty]
        public double Value { get; set; }


        // Gets the value of the form field instance passed from a view where the instance is rendered
        public override double GetValue()
        {
            return Value;
        }

        public override void SetValue(double value)
        {
            Value = value;
        }
    }
}