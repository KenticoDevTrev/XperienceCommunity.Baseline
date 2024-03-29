﻿using CMS.DataEngine;
using Kentico.Forms.Web.Mvc;

namespace Core.RCL.KX13.Models.FormComponents.DecimalInput
{
    public class DecimalInputComponentProperties : FormComponentProperties<decimal>
    {
        public decimal Value { get; set; }

        [DefaultValueEditingComponent(TextInputComponent.IDENTIFIER)]
        public override decimal DefaultValue { get => Value; set => Value = value; }

        bool DecimalPrecisionWasSet { get; set; } = false;
        bool DecimalSizeWasSet { get; set; } = false;
        [EditingComponentProperty(nameof(DecimalPrecision), 38)]
        public int DecimalPrecision
        {
            get {
                return (DecimalPrecisionWasSet ? Precision : -1);
            } set
            {
                this.Precision = value;
                DecimalPrecisionWasSet = true;
            }
        }

        [EditingComponentProperty(nameof(DecimalSize), 38)]
        public int DecimalSize
        {
            get
            {
                return (DecimalSizeWasSet ? Size : -1);
            }
            set
            {
                this.Size = value;
                DecimalSizeWasSet = true;
            }
        }

        // Initializes a new instance of the properties class and configures the underlying database field
        public DecimalInputComponentProperties()
            : base(FieldDataType.Decimal)
        {
        }

    }
}