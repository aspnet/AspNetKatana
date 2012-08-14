using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Owin.Builder.Utils
{
    internal static class Conversions
    {
        public static bool IsOwinDelegate(Type delegateType)
        {
            return
                IsAppDelegate(delegateType) ||
                IsAppAction(delegateType);
        }

        public static bool IsAppDelegate(Type delegateType)
        {
            var method = delegateType.GetMethod("Invoke");
            if (method == null)
            {
                return false;
            }

            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var returnType = method.ReturnType;

            if (parameterTypes.Length != 1)
            {
                return false;
            }
            if (!IsCallParameters(parameterTypes[0]))
            {
                return false;
            }
            if (!IsTaskOfResultParameters(returnType))
            {
                return false;
            }
            return true;
        }

        static bool IsCallParameters(Type type)
        {
            if (!type.IsValueType)
            {
                return false;
            }
            var fields = type.GetFields();
            if (fields.Length != 3)
            {
                return false;
            }
            var hasEnvironment = fields.Any(field => field.Name == "Environment" && field.FieldType == typeof(IDictionary<string, object>));
            var hasHeaders = fields.Any(field => field.Name == "Headers" && field.FieldType == typeof(IDictionary<string, string[]>));
            var hasBody = fields.Any(field => field.Name == "Body" && field.FieldType == typeof(Stream));
            return hasEnvironment && hasHeaders && hasBody;
        }

        static bool IsTaskOfResultParameters(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }
            if (type.GetGenericTypeDefinition() != typeof(Task<>))
            {
                return false;
            }
            var genericArgument = type.GetGenericArguments()[0];
            if (!IsResultParameters(genericArgument))
            {
                return false;
            }
            return true;
        }

        static bool IsResultParameters(Type type)
        {
            if (!type.IsValueType)
            {
                return false;
            }
            var fields = type.GetFields();
            if (fields.Length != 4)
            {
                return false;
            }
            var hasProperties = fields.Any(field => field.Name == "Properties" && field.FieldType == typeof(IDictionary<string, object>));
            var hasStatus = fields.Any(field => field.Name == "Status" && field.FieldType == typeof(int));
            var hasHeaders = fields.Any(field => field.Name == "Headers" && field.FieldType == typeof(IDictionary<string, string[]>));
            var hasBody = fields.Any(field => field.Name == "Body" && field.FieldType == typeof(Func<Stream, Task>));
            return hasProperties && hasStatus && hasHeaders && hasBody;
        }

        public static bool IsAppAction(Type delegateType)
        {
            var method = delegateType.GetMethod("Invoke");
            if (method == null)
            {
                return false;
            }

            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var returnType = method.ReturnType;

            if (parameterTypes.Length != 3)
            {
                return false;
            }
            if (!AreCallArguments(parameterTypes[0], parameterTypes[1], parameterTypes[2]))
            {
                return false;
            }
            if (!IsTaskOfResultTuple(returnType))
            {
                return false;
            }
            return true;
        }

        static bool AreCallArguments(Type arg0, Type arg1, Type arg2)
        {
            if (arg0 != typeof(IDictionary<string, object>))
            {
                return false;
            }
            if (arg1 != typeof(IDictionary<string, string[]>))
            {
                return false;
            }
            if (arg2 != typeof(Stream))
            {
                return false;
            }
            return true;
        }

        static bool IsTaskOfResultTuple(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }
            if (type.GetGenericTypeDefinition() != typeof(Task<>))
            {
                return false;
            }
            var genericArgument = type.GetGenericArguments()[0];
            if (!IsResultTuple(genericArgument))
            {
                return false;
            }
            return true;
        }

        static bool IsResultTuple(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }
            if (type.GetGenericTypeDefinition() != typeof(Tuple<,,,>))
            {
                return false;
            }
            var args = type.GetGenericArguments();
            if (args[0] != typeof(IDictionary<string, object>))
            {
                return false;
            }
            if (args[1] != typeof(int))
            {
                return false;
            }
            if (args[2] != typeof(IDictionary<string, string[]>))
            {
                return false;
            }
            if (args[3] != typeof(Func<Stream, Task>))
            {
                return false;
            }
            return true;
        }

        public static Func<object, object> EmitConversion(Type givenType, Type neededType)
        {
            if (neededType.IsAssignableFrom(givenType))
            {
                return app => app;
            }

            if (IsAppDelegate(givenType) && IsAppAction(neededType))
            {
                return EmitAppActionCallingAppDelegate(givenType, neededType);
            }
            else if (IsAppAction(givenType) && IsAppDelegate(neededType))
            {
                return EmitAppDelegateCallingAppAction(givenType, neededType);
            }
            else if (IsAppDelegate(givenType) && IsAppDelegate(neededType))
            {

            }
            return null;
        }

        static Func<object, object> EmitAppDelegateCallingAppAction(Type appActionType, Type appDelegateType)
        {
            //Expression<Func<AppAction, AppDelegate>> conversion =
            //    app => call =>
            //        app(call.Environment,
            //            call.Headers,
            //            call.Body).Then(result =>
            //                new ResultParameters
            //                {
            //                    Properties = result.Item1,
            //                    Status = result.Item2,
            //                    Headers = result.Item3,
            //                    Body = result.Item4,
            //                },
            //                default(CancellationToken),
            //                false);

            var thenMethod = typeof(TaskHelpersExtensions)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod)
                .Single(IsCorrectThenMethod);

            var appDelegateMethod = appDelegateType.GetMethod("Invoke");
            var taskOfResultParametersType = appDelegateMethod.ReturnType;
            var callParametersType = appDelegateMethod.GetParameters()[0].ParameterType;
            var resultParametersType = taskOfResultParametersType.GetGenericArguments()[0];

            var appActionMethod = appActionType.GetMethod("Invoke");
            var resultTupleType = appActionMethod.ReturnType.GetGenericArguments()[0];

            var appParameter = Expression.Parameter(appActionType, "app");
            var callParameter = Expression.Parameter(callParametersType, "call");
            var resultParameter = Expression.Parameter(resultTupleType, "result");

            var appInvoke = Expression.Invoke(
                appParameter,
                Expression.Field(callParameter, "Environment"),
                Expression.Field(callParameter, "Headers"),
                Expression.Field(callParameter, "Body"));

            var newResultParameters = Expression.MemberInit(
                Expression.New(resultParametersType),
                Expression.Bind(
                    resultParametersType.GetField("Properties"),
                    Expression.Property(resultParameter, "Item1")),
                Expression.Bind(
                    resultParametersType.GetField("Status"),
                    Expression.Property(resultParameter, "Item2")),
                Expression.Bind(
                    resultParametersType.GetField("Headers"),
                    Expression.Property(resultParameter, "Item3")),
                Expression.Bind(
                    resultParametersType.GetField("Body"),
                    Expression.Property(resultParameter, "Item4")));

            var resultLambda = Expression.Lambda(
                Expression.GetFuncType(resultTupleType, resultParametersType),
                newResultParameters,
                resultParameter);

            var thenInvoke = Expression.Call(
                thenMethod.MakeGenericMethod(resultTupleType, resultParametersType),
                appInvoke,
                resultLambda,
                Expression.Constant(default(CancellationToken), typeof(CancellationToken)),
                Expression.Constant(false, typeof(bool)));

            var appLambda = Expression.Lambda(
                appDelegateType,
                thenInvoke,
                callParameter);

            var conversionLambda = Expression.Lambda(
                Expression.GetFuncType(appActionType, appDelegateType),
                appLambda,
                appParameter);

            var conversion = conversionLambda.Compile();

            return app => conversion.DynamicInvoke(app);
        }

        static Func<object, object> EmitAppActionCallingAppDelegate(Type appDelegateType, Type appActionType)
        {
            //Expression<Func<AppDelegate, AppAction>> conversion =
            //    app => (env, headers, body) =>
            //        app(new CallParameters
            //        {
            //            Environment = env,
            //            Headers = headers,
            //            Body = body
            //        }).Then(result =>
            //            new ResultTuple(
            //                result.Properties,
            //                result.Status,
            //                result.Headers,
            //                result.Body
            //                ),
            //            default(CancellationToken),
            //            false);

            var thenMethod = typeof(TaskHelpersExtensions)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod)
                .Single(IsCorrectThenMethod);

            var appDelegateMethod = appDelegateType.GetMethod("Invoke");
            var taskOfResultParametersType = appDelegateMethod.ReturnType;
            var callParametersType = appDelegateMethod.GetParameters()[0].ParameterType;
            var resultParametersType = taskOfResultParametersType.GetGenericArguments()[0];

            var appActionMethod = appActionType.GetMethod("Invoke");
            var resultTupleType = appActionMethod.ReturnType.GetGenericArguments()[0];

            var appParameter = Expression.Parameter(appDelegateType, "app");
            var envParameter = Expression.Parameter(typeof(IDictionary<string, object>), "env");
            var headersParameter = Expression.Parameter(typeof(IDictionary<string, string[]>), "headers");
            var bodyParameter = Expression.Parameter(typeof(Stream), "body");
            var resultParameter = Expression.Parameter(resultParametersType, "result");

            var newCallParameters = Expression.MemberInit(
                Expression.New(callParametersType),
                Expression.Bind(callParametersType.GetField("Environment"), envParameter),
                Expression.Bind(callParametersType.GetField("Headers"), headersParameter),
                Expression.Bind(callParametersType.GetField("Body"), bodyParameter));

            var appInvoke = Expression.Invoke(
                appParameter,
                newCallParameters);

            var newTuple = Expression.New(
                resultTupleType.GetConstructors().Single(),
                Expression.Field(resultParameter, "Properties"),
                Expression.Field(resultParameter, "Status"),
                Expression.Field(resultParameter, "Headers"),
                Expression.Field(resultParameter, "Body"));

            var resultLambda = Expression.Lambda(
                Expression.GetFuncType(resultParametersType, resultTupleType),
                newTuple,
                resultParameter);

            var thenInvoke = Expression.Call(
                thenMethod.MakeGenericMethod(resultParametersType, resultTupleType),
                appInvoke,
                resultLambda,
                Expression.Constant(default(CancellationToken), typeof(CancellationToken)),
                Expression.Constant(false, typeof(bool)));

            var appLambda = Expression.Lambda(
                appActionType,
                thenInvoke,
                envParameter,
                headersParameter,
                bodyParameter);

            var conversionLambda = Expression.Lambda(
                Expression.GetFuncType(appDelegateType, appActionType),
                appLambda,
                appParameter);

            var conversion = conversionLambda.Compile();

            return app => conversion.DynamicInvoke(app);
        }

        static bool IsCorrectThenMethod(MethodInfo method)
        {
            if (method.Name != "Then")
            {
                return false;
            }
            if (!method.IsGenericMethodDefinition)
            {
                return false;
            }
            var genericArguments = method.GetGenericArguments();
            if (genericArguments.Length != 2)
            {
                return false;
            }
            var parameters = method.GetParameters();
            if (parameters.Length != 4)
            {
                return false;
            }
            var funcType = parameters[1].ParameterType;
            if (!funcType.IsGenericType)
            {
                return false;
            }
            var funcGenericArguments = funcType.GetGenericArguments();
            if (funcGenericArguments.Length != 2)
            {
                return false;
            }
            if (funcGenericArguments[1] != genericArguments[1])
            {
                return false;
            }
            return true;
        }
    }
}

