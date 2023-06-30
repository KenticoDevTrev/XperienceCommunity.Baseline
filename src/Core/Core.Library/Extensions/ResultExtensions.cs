using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class ResultExtensions
    {

        public static Maybe<T> AsMaybeIfSuccessful<T>(this Result<T> value)
        {
            return value.IsSuccess ? value.Value : Maybe.None;
        }
    }
}
