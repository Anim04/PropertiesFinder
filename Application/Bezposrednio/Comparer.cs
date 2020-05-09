using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Application.OfertyDom
{
    class Comparer : IEqualityComparer<Entry>
    {
        public bool Equals([AllowNull] Entry x, [AllowNull] Entry y)
        {
            if (GetHashCode(x) == GetHashCode(y))
                return true;
            return false;
        }

        public int GetHashCode([DisallowNull] Entry obj)
        {
            return obj.PropertyPrice.TotalGrossPrice.GetHashCode() +
                   obj.PropertyPrice.PricePerMeter.GetHashCode() +
                   obj.PropertyAddress.City.GetHashCode() +
                   obj.PropertyAddress.StreetName.GetHashCode() +
                   obj.PropertyAddress.District.GetHashCode() +
                   obj.PropertyDetails.Area.GetHashCode();
        }
    }
}
