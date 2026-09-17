using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace CustomerManagementSystem.core.backend.Configurations
{
    /// <summary>
    /// Tự động thêm tiền tố (prefix) cho toàn bộ Controller trong hệ thống
    /// Vd: prefix "api/v1" + route "admin/customers" => "api/v1/admin/customers"
    /// </summary>
    public class RoutePrefixConvention : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _routePrefix;

        public RoutePrefixConvention(IRouteTemplateProvider route)
        {
            _routePrefix = new AttributeRouteModel(route);
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var selector in application.Controllers.SelectMany(c => c.Selectors))
            {
                if (selector.AttributeRouteModel != null)
                {
                    selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_routePrefix, selector.AttributeRouteModel);
                }
                else
                {
                    selector.AttributeRouteModel = _routePrefix;
                }
            }
        }
    }
}
