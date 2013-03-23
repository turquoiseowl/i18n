using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using i18n.DataAnnotations;

namespace i18n
{
    public class I18nModelMetadataProvider : DataAnnotationsModelMetadataProvider
    {
        protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes,
                                                        Type containerType,
                                                        Func<object> modelAccessor,
                                                        Type modelType,
                                                        string propertyName)
        {

            ModelMetadata metadata = base.CreateMetadata(attributes,
                                                         containerType,
                                                         modelAccessor,
                                                         modelType,
                                                         propertyName);

            DisplayAttribute display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            if (display != null)
            {
                string name = display.Name;
                if (name != null)
                {
                    metadata.DisplayName = name;
                }

                metadata.Description = display.Description;
                metadata.Watermark = display.Prompt;
            }

            return metadata;
        }
    }
}