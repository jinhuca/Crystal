﻿
#if !PCL && !NET35 && !NET40 && !NET403 && !NETSTANDARD1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_2 && !NETCOREAPP1_0 && !NETCOREAPP1_1
#define SUPPORTS_FAST_EXPRESSION_COMPILER
#endif
#if !PCL && !NET35 && !NET40 && !NET403 && !NETSTANDARD1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_2 && !NETSTANDARD1_3 && !NETSTANDARD1_4 && !NETSTANDARD1_5 && !NETSTANDARD1_6 && !NETCOREAPP1_0 && !NETCOREAPP1_1
#define SUPPORTS_ISERVICE_PROVIDER
#endif
#if !PCL && !NETSTANDARD1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_2 && !NETSTANDARD1_3 && !NETSTANDARD1_4 && !NETSTANDARD1_5 && !NETSTANDARD1_6
#define SUPPORTS_SERIALIZABLE
#define SUPPORTS_ICLONEABLE
#define SUPPORTS_STACK_TRACE
#define SUPPORTS_MANAGED_THREAD_ID
#endif
#if !PCL && !NETSTANDARD1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_2 && !NETSTANDARD1_3 && !NETSTANDARD1_5 && !NET35 && !NET40 && !NET403 && !NET45 && !NET451 && !NET452
#define SUPPORTS_ASYNC_LOCAL
#endif
#if !PCL && !NETSTANDARD1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_2 && !NET35 && !NET40 && !NET403
#define SUPPORTS_VARIANCE
#endif
#if !PCL && !NET35 && !NET40 && !NET403 && !NET45 && !NET451 && !NET452 && !NET46 && !NET461 && !NET462 && !NET47 && !NET471 && !NET472 && !NETSTANDARD1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_2 && !NETSTANDARD1_3 && !NETSTANDARD1_4
#define SUPPORTS_EXPRESSION_COMPILE_WITH_PREFER_INTERPRETATION_PARAM
#endif
#if !PCL && !NET35 && !NET40 && !NET403
#define SUPPORTS_DELEGATE_METHOD
#endif
#if !NET35 && !PCL
#define SUPPORTS_SPIN_WAIT
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Diagnostics.CodeAnalysis;  // for SuppressMessage
using System.Diagnostics;               // for StackTrace
using System.Runtime.CompilerServices;  // for MethodImplAttribute

using Crystal.Utilities;
using static Crystal.Utilities.ArrayTools;
using static System.Environment;

using ExprType = System.Linq.Expressions.ExpressionType;

#if SUPPORTS_FAST_EXPRESSION_COMPILER
using FastExpressionCompiler.LightExpression;
using static FastExpressionCompiler.LightExpression.Expression;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.Expression;
#endif

namespace Crystal
{
	/// <summary>Inversion of control container</summary>
	public partial class Container : IContainer
  {
    /// <summary>Creates new container with default rules <see cref="Injector.Rules.Default"/>.</summary>
    public Container() : this(Rules.Default, Ref.Of(Registry.Default), NewSingletonScope()) =>
        SetInitialFactoryID();

    /// <summary>Creates new container, optionally providing <see cref="Rules"/> to modify default container behavior.</summary>
    /// <param name="rules">(optional) Rules to modify container default resolution behavior.
    /// If not specified, then <see cref="Injector.Rules.Default"/> will be used.</param>
    /// <param name="scopeContext">(optional) Scope context to use for scoped reuse.</param>
    public Container(Rules rules = null, IScopeContext scopeContext = null)
        : this(rules ?? Rules.Default, Ref.Of(Registry.Default), NewSingletonScope(), scopeContext) =>
        SetInitialFactoryID();

    /// <summary>Creates new container with configured rules.</summary>
    /// <param name="configure">Allows to modify <see cref="Injector.Rules.Default"/> rules.</param>
    /// <param name="scopeContext">(optional) Scope context to use for <see cref="Reuse.InCurrentScope"/>.</param>
    public Container(Func<Rules, Rules> configure, IScopeContext scopeContext = null)
        : this(configure.ThrowIfNull()(Rules.Default) ?? Rules.Default, scopeContext)
    { }

    /// <summary>Helper to create singleton scope</summary>
    public static IScope NewSingletonScope() => new Scope(name: "<singletons>");

    /// <summary>Pretty prints the container info including the open scope details if any.</summary> 
    public override string ToString()
    {
      var s = _scopeContext == null ? "container" : "container with ambient " + _scopeContext;

      var scope = CurrentScope;
      s += scope == null ? " without scope" : " with scope " + scope;

      if (Rules != Rules.Default)
        s += NewLine + " with " + Rules;

      if (IsDisposed)
      {
        s += " has been DISPOSED!" + NewLine;
        if (_disposeStackTrace != null)
          s += " Dispose stack-trace " + _disposeStackTrace;
        else
          s += " You may include Dispose stack-trace into the message via:" + NewLine +
              "container.With(rules => rules.WithCaptureContainerDisposeStackTrace())";
      }

      return s;
    }

    /// <summary>Dispose either open scope, or container with singletons, if no scope opened.</summary>
    public void Dispose()
    {
      // if already disposed - just leave
      if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
        return;

      // Nice to have disposal stack-trace, but we can live without it if something goes wrong
      if (Rules.CaptureContainerDisposeStackTrace)
        try { _disposeStackTrace = new StackTrace(); }
        catch { }

      if (_parent != null)
      {
        if (_ownCurrentScope != null)
        {
          _ownCurrentScope.Dispose();
        }
        else if (_scopeContext != null)
        {
          IScope currentScope = null;
          _scopeContext.SetCurrent(s =>
          {
            // save the current scope for the later,
            // do dispose it AFTER its parent is actually set to be a new ambient current scope.
            currentScope = s;
            return s?.Parent;
          });
          currentScope?.Dispose();
        }
      }
      else
      {
        _registry.Swap(Registry.Empty);
        Rules = Rules.Default;
        _singletonScope.Dispose(); // will also dispose any tracked scopes
        _scopeContext?.Dispose();
      }
    }

    #region Compile-time generated parts - former DryIocZero

    partial void GetLastGeneratedFactoryID(ref int lastFactoryID);

    partial void ResolveGenerated(ref object service, Type serviceType);

    partial void ResolveGenerated(ref object service,
        Type serviceType, object serviceKey, Type requiredServiceType, Request preRequestParent, object[] args);

    partial void ResolveManyGenerated(ref IEnumerable<ResolveManyResult> services, Type serviceType);

    /// <summary>Identifies the service when resolving collection</summary>
    public struct ResolveManyResult
    {
      /// <summary>Factory, the required part</summary>
      public FactoryDelegate FactoryDelegate;

      /// <summary>Optional key</summary>
      public object ServiceKey;

      /// <summary>Optional required service type, can be an open-generic type.</summary>
      public Type RequiredServiceType;

      /// <summary>Constructs the struct.</summary>
      public static ResolveManyResult Of(FactoryDelegate factoryDelegate,
          object serviceKey = null, Type requiredServiceType = null) =>
          new ResolveManyResult
          {
            FactoryDelegate = factoryDelegate,
            ServiceKey = serviceKey,
            RequiredServiceType = requiredServiceType
          };
    }

    /// <summary>Directly uses generated factories to resolve service. Or returns the default if service does not have generated factory.</summary>
    [SuppressMessage("ReSharper", "InvocationIsSkipped", Justification = "Per design")]
    [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull", Justification = "Per design")]
    public object ResolveCompileTimeGeneratedOrDefault(Type serviceType)
    {
      object service = null;
      ResolveGenerated(ref service, serviceType);
      return service;
    }

    /// <summary>Directly uses generated factories to resolve service. Or returns the default if service does not have generated factory.</summary>
    [SuppressMessage("ReSharper", "InvocationIsSkipped", Justification = "Per design")]
    [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull", Justification = "Per design")]
    public object ResolveCompileTimeGeneratedOrDefault(Type serviceType, object serviceKey)
    {
      object service = null;
      ResolveGenerated(ref service, serviceType, serviceKey,
          requiredServiceType: null, preRequestParent: null, args: null);
      return service;
    }

    /// <summary>Resolves many generated only services. Ignores runtime registrations.</summary>
    public IEnumerable<ResolveManyResult> ResolveManyCompileTimeGeneratedOrEmpty(Type serviceType)
    {
      IEnumerable<ResolveManyResult> manyGenerated = ArrayTools.Empty<ResolveManyResult>();
      ResolveManyGenerated(ref manyGenerated, serviceType);
      return manyGenerated;
    }

    #endregion

    #region IRegistrator

    /// <summary>Returns all registered service factories with their Type and optional Key.</summary>
    /// <remarks>Decorator and Wrapper types are not included.</remarks>
    public IEnumerable<ServiceRegistrationInfo> GetServiceRegistrations() =>
        _registry.Value.GetServiceRegistrations();

    // todo: Make `serviceKey` and `factoryType` optional
    /// <summary>Searches for registered factories by type, and key (if specified),
    /// and by factory type (by default uses <see cref="FactoryType.Service"/>).
    /// May return empty, 1 or multiple factories.</summary>
    public Factory[] GetRegisteredFactories(Type serviceType, object serviceKey, FactoryType factoryType) =>
        _registry.Value.GetRegisteredFactories(serviceType.ThrowIfNull(), serviceKey, factoryType);

    /// <summary>Stores factory into container using <paramref name="serviceType"/> and <paramref name="serviceKey"/> as key
    /// for later lookup.</summary>
    /// <param name="factory">Any subtypes of <see cref="Factory"/>.</param>
    /// <param name="serviceType">Type of service to resolve later.</param>
    /// <param name="serviceKey">(optional) Service key of any type with <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>
    /// implemented.</param>
    /// <param name="ifAlreadyRegistered">(optional) Says how to handle existing registration with the same
    /// <paramref name="serviceType"/> and <paramref name="serviceKey"/>.</param>
    /// <param name="isStaticallyChecked">Confirms that service and implementation types are statically checked by compiler.</param>
    /// <returns>True if factory was added to registry, false otherwise.
    /// False may be in case of <see cref="IfAlreadyRegistered.Keep"/> setting and already existing factory.</returns>
    public void Register(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered? ifAlreadyRegistered, bool isStaticallyChecked)
    {
      ThrowIfContainerDisposed();

      if (serviceKey == null)
        serviceKey = Rules.DefaultRegistrationServiceKey;

      factory.ThrowIfNull().ValidateAndNormalizeRegistration(serviceType, serviceKey, isStaticallyChecked, Rules);

      if (!ifAlreadyRegistered.HasValue)
        ifAlreadyRegistered = Rules.DefaultIfAlreadyRegistered;

      // Improves performance a bit by first attempting to swap the registry while it is still unchanged.
      var r = _registry.Value;
      var st = serviceType.GetTypeInfo();
      if (st.IsGenericType && !st.IsGenericTypeDefinition && st.ContainsGenericParameters)
        serviceType = serviceType.GetGenericTypeDefinition();
      if (!_registry.TrySwapIfStillCurrent(r, r.Register(factory, serviceType, ifAlreadyRegistered.Value, serviceKey)))
        RegistrySwap(factory, serviceType, serviceKey, ifAlreadyRegistered);
    }

    // hiding nested lambda in method to reduce allocations
    private Registry RegistrySwap(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered? ifAlreadyRegistered) =>
        _registry.Swap(r => r.Register(factory, serviceType, ifAlreadyRegistered.Value, serviceKey));

    /// <inheritdoc />
    public bool IsRegistered(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition)
    {
      ThrowIfContainerDisposed();
      return _registry.Value.IsRegistered(serviceType, serviceKey, factoryType, condition);
    }

    /// <inheritdoc />
    public void Unregister(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition)
    {
      ThrowIfContainerDisposed();
      _registry.Swap(r => r.Unregister(factoryType, serviceType, serviceKey, condition));
    }

    #endregion

    #region IResolver

#if SUPPORTS_ISERVICE_PROVIDER
    /// <summary>
    /// Resolves service with the <see cref="IfUnresolved.ReturnDefaultIfNotRegistered"/> policy,
    /// enabling the fallback resolution for not registered services (default MS convention).
    /// For diagnostics reasons, you may globally set the rule <see cref="Injector.Rules.ServiceProviderGetServiceShouldThrowIfUnresolved"/> to alter the behavior. 
    /// It may help to highlight the issues by throwing the original rich <see cref="ContainerException"/> instead of just returning the `null`.
    /// </summary>
    object IServiceProvider.GetService(Type serviceType) =>
        ((IResolver)this).Resolve(serviceType,
            Rules.ServiceProviderGetServiceShouldThrowIfUnresolved ? IfUnresolved.Throw : IfUnresolved.ReturnDefaultIfNotRegistered);
#endif

    object IResolver.Resolve(Type serviceType, IfUnresolved ifUnresolved)
    {
      object service = null;
      ResolveGenerated(ref service, serviceType);
      if (service != null)
        return service;

      var serviceTypeHash = RuntimeHelpers.GetHashCode(serviceType);
      var cacheEntry = _registry.Value.GetCachedDefaultFactoryOrDefault(serviceTypeHash, serviceType);
      if (cacheEntry != null)
      {
        ref var entry = ref cacheEntry.Value;
        if (entry.Value is FactoryDelegate cachedDelegate)
          return cachedDelegate(this);

        if (ResolverContext.TryGetUsedInstance(this, serviceType, out var usedInstance))
        {
          entry.Value = null; // reset the cache
          return usedInstance;
        }

        var rules = Rules;
        while (entry.Value is Expression expr)
        {
          if (rules.UseInterpretation &&
              Interpreter.TryInterpretAndUnwrapContainerException(this, expr, false, out var result))
            return result;

          // set to Compiling to notify other threads to use the interpretation until the service is compiled
          if (Interlocked.CompareExchange(ref entry.Value, new Registry.Compiling(expr), expr) == expr)
          {
            var compiledFactory = expr.CompileToFactoryDelegate(rules.UseFastExpressionCompiler, rules.UseInterpretation);
            entry.Value = compiledFactory; // todo: @unclear should we instead cache only after invoking the factory delegate
            return compiledFactory(this);
          }
        }

        if (entry.Value is Registry.Compiling compiling)
        {
          if (Interpreter.TryInterpretAndUnwrapContainerException(this, compiling.Expression, false, out var result))
            return result;
          return compiling.Expression.CompileToFactoryDelegate(rules.UseFastExpressionCompiler, rules.UseInterpretation)(this);
        }
      }

      return ResolveAndCache(serviceTypeHash, serviceType, ifUnresolved);
    }

    private object ResolveAndCache(int serviceTypeHash, Type serviceType, IfUnresolved ifUnresolved)
    {
      ThrowIfContainerDisposed();

      if (ResolverContext.TryGetUsedInstance(this, serviceType, out var usedInstance))
        return usedInstance;

      var request = Request.Create(this, serviceType, ifUnresolved: ifUnresolved);
      var factory = ((IContainer)this).ResolveFactory(request); // HACK: may mutate request, but it should be safe

      // Delegate to full blown Resolve aware of service key, open scope, etc.
      var serviceKey = request.ServiceKey;
      var scopeName = CurrentScope?.Name;
      if (serviceKey != null || scopeName != null)
        return ResolveAndCacheKeyed(serviceTypeHash, serviceType, serviceKey, ifUnresolved, scopeName, null, Request.Empty, null);

      if (factory == null)
        return null;

      var rules = Rules;
      FactoryDelegate factoryDelegate;

      // todo: [Obsolete] - in v5.0 there should be no check nor the InstanceFactory
      if (factory is InstanceFactory ||
          !rules.UseInterpretationForTheFirstResolution)
      {
        factoryDelegate = factory.GetDelegateOrDefault(request);
        if (factoryDelegate == null)
          return null;
      }
      else
      {
        var expr = factory.GetExpressionOrDefault(request);
        if (expr == null)
          return null;

        if (expr is ConstantExpression constExpr)
        {
          var value = constExpr.Value;
          if (factory.Caching != FactoryCaching.DoNotCache)
            _registry.Value.TryCacheDefaultFactory<FactoryDelegate>(serviceTypeHash, serviceType, value.ToFactoryDelegate);
          return value;
        }

        // Important to cache expression first before tying to interpret,
        // so that parallel resolutions may already use it and UseInstance may correctly evict the cache if needed.
        if (factory.Caching != FactoryCaching.DoNotCache)
          _registry.Value.TryCacheDefaultFactory(serviceTypeHash, serviceType, expr);

        // 1) First try to interpret
        if (Interpreter.TryInterpretAndUnwrapContainerException(this, expr, rules.UseFastExpressionCompiler, out var instance))
          return instance;
        // 2) Fallback to expression compilation
        factoryDelegate = expr.CompileToFactoryDelegate(rules.UseFastExpressionCompiler, rules.UseInterpretation);
      }

      if (factory.Caching != FactoryCaching.DoNotCache)
        _registry.Value.TryCacheDefaultFactory(serviceTypeHash, serviceType, factoryDelegate);

      return factoryDelegate(this);
    }

    object IResolver.Resolve(Type serviceType, object serviceKey,
        IfUnresolved ifUnresolved, Type requiredServiceType, Request preResolveParent, object[] args)
    {
      // fallback to simple Resolve and its default cache if no keys are passed
      var scopeName = CurrentScope?.Name;
      if (serviceKey == null && requiredServiceType == null && scopeName == null &&
          (preResolveParent == null || preResolveParent.IsEmpty) && args.IsNullOrEmpty())
        return ((IResolver)this).Resolve(serviceType, ifUnresolved);

      var service = ResolveAndCacheKeyed(RuntimeHelpers.GetHashCode(serviceType), serviceType,
          serviceKey, ifUnresolved, scopeName, requiredServiceType, preResolveParent ?? Request.Empty, args);

      return service;
    }

    private object ResolveAndCacheKeyed(int serviceTypeHash, Type serviceType,
        object serviceKey, IfUnresolved ifUnresolved, object scopeName, Type requiredServiceType, Request preResolveParent,
        object[] args)
    {
      object service = null;
      ResolveGenerated(ref service, serviceType, serviceKey, requiredServiceType, preResolveParent, args);
      if (service != null)
        return service;

      // #288 - ignoring the parent, `args`, `scopeName`seems OK because Use is supposed to overwrite anything with args,
      // and TryGetUsedInstance will look into scope with the specified `scopeName` anyway
      if (serviceKey == null && requiredServiceType == null)
        if (ResolverContext.TryGetUsedInstance(this, serviceType, out var usedInstance))
          return usedInstance;

      object cacheKey = null;
      if (requiredServiceType == null && preResolveParent.IsEmpty && args.IsNullOrEmpty())
      {
        cacheKey = scopeName == null ? serviceKey
            : serviceKey == null ? scopeName
            : KV.Of(scopeName, serviceKey);

        if (_registry.Value.GetCachedKeyedFactoryOrDefault(serviceTypeHash, serviceType, cacheKey, out var cacheEntry))
        {
          if (cacheEntry.Factory is FactoryDelegate cachedDelegate)
            return cachedDelegate(this);
          if (TryInterpretOrCompileCachedExpression(this, cacheEntry, Rules, out var result))
            return result;
        }
      }

      // Cache is missed, so get the factory and put it into cache:
      ThrowIfContainerDisposed();

      var request = Request.Create(this, serviceType, serviceKey, ifUnresolved, requiredServiceType, preResolveParent, default, args);
      var factory = ((IContainer)this).ResolveFactory(request);
      if (factory == null)
        return null;

      // Prevents caching if factory says Don't
      if (factory.Caching == FactoryCaching.DoNotCache)
        cacheKey = null;

      // Request service key may be changed when resolving the factory,
      // so we need to look into Default cache again for the new key
      if (cacheKey != null && serviceKey == null && request.ServiceKey != null)
      {
        cacheKey = scopeName == null ? request.ServiceKey : KV.Of(scopeName, request.ServiceKey);
        if (_registry.Value.GetCachedKeyedFactoryOrDefault(serviceTypeHash, serviceType, cacheKey, out var cacheEntry))
        {
          if (cacheEntry.Factory is FactoryDelegate cachedDelegate)
            return cachedDelegate(this);
          if (TryInterpretOrCompileCachedExpression(this, cacheEntry, Rules, out var result))
            return result;
        }
      }

      FactoryDelegate factoryDelegate;
      if (factory is InstanceFactory || !Rules.UseInterpretationForTheFirstResolution)
      {
        factoryDelegate = factory.GetDelegateOrDefault(request);
        if (factoryDelegate == null)
          return null;
      }
      else
      {
        var expr = factory.GetExpressionOrDefault(request);
        if (expr == null)
          return null;

        if (expr is ConstantExpression constExpr)
        {
          var value = constExpr.Value;
          if (cacheKey != null)
            _registry.Value.TryCacheKeyedFactory(serviceTypeHash, serviceType, cacheKey, (FactoryDelegate)value.ToFactoryDelegate);
          return value;
        }

        // Important to cache expression first before tying to interpret,
        // so that parallel resolutions may already use it and UseInstance may correctly evict the cache if needed
        if (cacheKey != null)
          _registry.Value.TryCacheKeyedFactory(serviceTypeHash, serviceType, cacheKey, expr);

        // 1) First try to interpret
        var useFec = Rules.UseFastExpressionCompiler;
        if (Interpreter.TryInterpretAndUnwrapContainerException(this, expr, useFec, out var instance))
          return instance;

        // 2) Fallback to expression compilation
        factoryDelegate = expr.CompileToFactoryDelegate(useFec, Rules.UseInterpretation);
      }

      // Cache factory only when we successfully called the factory delegate, to prevent failing delegates to be cached.
      // Additionally disable caching when no services registered, not to cache an empty collection wrapper or alike.
      if (cacheKey != null)
        _registry.Value.TryCacheKeyedFactory(serviceTypeHash, serviceType, cacheKey, factoryDelegate);

      return factoryDelegate(this);
    }

    private static bool TryInterpretOrCompileCachedExpression(IResolverContext r,
        Registry.KeyedFactoryCacheEntry cacheEntry, Rules rules, out object result)
    {
      while (cacheEntry.Factory is Expression expr)
      {
        if (rules.UseInterpretation &&
            Interpreter.TryInterpretAndUnwrapContainerException(r, expr, false, out result))
          return true;

        // set to Compiling to notify other threads to use the interpretation until the service is compiled
        if (Interlocked.CompareExchange(ref cacheEntry.Factory, new Registry.Compiling(expr), expr) == expr)
        {
          var factoryDelegate = expr.CompileToFactoryDelegate(rules.UseFastExpressionCompiler, rules.UseInterpretation);
          // todo: should we instead cache only after invoking the factory delegate
          cacheEntry.Factory = factoryDelegate;
          result = factoryDelegate(r);
          return true;
        }
      }

      if (cacheEntry.Factory is Registry.Compiling compiling)
      {
        if (!Interpreter.TryInterpretAndUnwrapContainerException(r, compiling.Expression, false, out result))
          result = compiling.Expression.CompileToFactoryDelegate(rules.UseFastExpressionCompiler, rules.UseInterpretation)(r);
        return true;
      }

      result = null;
      return false;
    }

    IEnumerable<object> IResolver.ResolveMany(Type serviceType, object serviceKey,
        Type requiredServiceType, Request preResolveParent, object[] args)
    {
      var requiredItemType = requiredServiceType ?? serviceType;

      // first return compile-time generated factories if any
      var generatedFactories = Enumerable.Empty<ResolveManyResult>();
      ResolveManyGenerated(ref generatedFactories, serviceType);
      if (serviceKey != null)
        generatedFactories = generatedFactories.Where(x => serviceKey.Equals(x.ServiceKey));
      if (requiredServiceType != null)
        generatedFactories = generatedFactories.Where(x => requiredServiceType == x.RequiredServiceType);

      foreach (var generated in generatedFactories)
        yield return generated.FactoryDelegate(this);

      // Emulating the collection parent so that collection related rules and conditions were applied
      // the same way as if resolving IEnumerable<T>
      if (preResolveParent == null || preResolveParent.IsEmpty)
        preResolveParent = Request.Empty.Push(
            typeof(IEnumerable<object>), requiredItemType, serviceKey, IfUnresolved.Throw,
            WrappersSupport.CollectionWrapperID, FactoryType.Wrapper, null, null, 0, 0);

      var container = (IContainer)this;

      var unwrappedType = container.GetWrappedType(requiredItemType, null);
      if (unwrappedType != null && unwrappedType != typeof(void)) // accounting for the resolved action GH#114
        requiredItemType = unwrappedType;

      var items = container.GetServiceRegisteredAndDynamicFactories(requiredItemType)
          .Where(x => x.Value != null) // filter out unregistered services
          .Select(f => new ServiceRegistrationInfo(f.Value, requiredServiceType, f.Key));

      IEnumerable<ServiceRegistrationInfo> openGenericItems = null;
      if (requiredItemType.IsClosedGeneric())
      {
        var requiredItemOpenGenericType = requiredItemType.GetGenericDefinitionOrNull();
        openGenericItems = container.GetServiceRegisteredAndDynamicFactories(requiredItemOpenGenericType)
            .Where(x => x.Value != null)
            .Select(x => new ServiceRegistrationInfo(x.Value, requiredServiceType,
                new OpenGenericTypeKey(requiredItemOpenGenericType, x.Key)));
      }

      // Append registered generic types with compatible variance,
      // e.g. for IHandler<in E> - IHandler<A> is compatible with IHandler<B> if B : A.
      IEnumerable<ServiceRegistrationInfo> variantGenericItems = null;
      if (requiredItemType.IsGeneric() && container.Rules.VariantGenericTypesInResolvedCollection)
      {
        variantGenericItems = container.GetServiceRegistrations()
            .Where(x => x.ServiceType.IsGeneric()
                && x.ServiceType.GetGenericTypeDefinition() == requiredItemType.GetGenericTypeDefinition()
                && x.ServiceType != requiredItemType
                && x.ServiceType.IsAssignableTo(requiredItemType));
      }

      if (serviceKey != null) // include only single item matching key.
      {
        items = items.Where(it => serviceKey.Equals(it.OptionalServiceKey));
        if (openGenericItems != null)
          openGenericItems = openGenericItems // extract the actual key from combined type and key
              .Where(x => serviceKey.Equals(((OpenGenericTypeKey)x.OptionalServiceKey).ServiceKey));
        if (variantGenericItems != null)
          variantGenericItems = variantGenericItems
              .Where(it => serviceKey.Equals(it.OptionalServiceKey));
      }

      var metadataKey = preResolveParent.MetadataKey;
      var metadata = preResolveParent.Metadata;
      if (metadataKey != null || metadata != null)
      {
        items = items.Where(x => x.Factory.Setup.MatchesMetadata(metadataKey, metadata));
        if (openGenericItems != null)
          openGenericItems = openGenericItems.Where(x => x.Factory.Setup.MatchesMetadata(metadataKey, metadata));
        if (variantGenericItems != null)
          variantGenericItems = variantGenericItems.Where(x => x.Factory.Setup.MatchesMetadata(metadataKey, metadata));
      }

      // Exclude composite parent service from items, skip decorators
      var parent = preResolveParent;
      if (parent.FactoryType != FactoryType.Service)
        parent = parent.FirstOrDefault(p => p.FactoryType == FactoryType.Service) ?? Request.Empty;

      if (!parent.IsEmpty && parent.GetActualServiceType() == requiredItemType)
      {
        items = items.Where(x => x.Factory.FactoryID != parent.FactoryID);

        if (openGenericItems != null)
          openGenericItems = openGenericItems.Where(x => x
              .Factory.FactoryGenerator?.GeneratedFactories.Enumerate()
              .FindFirst(f => f.Value.FactoryID == parent.FactoryID) == null);

        if (variantGenericItems != null)
          variantGenericItems = variantGenericItems
              .Where(x => x.Factory.FactoryID != parent.FactoryID);
      }

      var allItems = openGenericItems == null && variantGenericItems == null ? items
          : variantGenericItems == null ? items.Concat(openGenericItems)
          : openGenericItems == null ? items.Concat(variantGenericItems)
          : items.Concat(openGenericItems).Concat(variantGenericItems);

      // Resolve in registration order
      foreach (var item in allItems.OrderBy(x => x.FactoryRegistrationOrder))
      {
        var service = container.Resolve(serviceType, item.OptionalServiceKey,
            IfUnresolved.ReturnDefaultIfNotRegistered, item.ServiceType, preResolveParent, args);
        if (service != null) // skip unresolved items
          yield return service;
      }
    }

    private void ThrowIfContainerDisposed()
    {
      if (IsDisposed)
        Throw.It(Error.ContainerIsDisposed, ToString());
    }

    #endregion

    #region IResolverContext

    /// <inheritdoc />
    public IResolverContext Parent => _parent;

    /// <inheritdoc />
    public IResolverContext Root
    {
      get
      {
        if (_parent == null)
          return null;
        var p = _parent;
        while (p.Parent != null)
          p = p.Parent;
        return p;
      }
    }

    /// <inheritdoc />
    public IScope SingletonScope => _singletonScope;

    /// <inheritdoc />
    public IScopeContext ScopeContext => _scopeContext;

    /// <inheritdoc />
    public IScope CurrentScope =>
        _scopeContext == null ? _ownCurrentScope : _scopeContext.GetCurrentOrDefault();

    /// <inheritdoc />
    [MethodImpl((MethodImplOptions)256)]
    public IResolverContext WithCurrentScope(IScope scope)
    {
      ThrowIfContainerDisposed();
      return new Container(Rules, _registry, _singletonScope, _scopeContext,
          scope, _disposed, _disposeStackTrace, parent: this);
    }

    /// [Obsolete("Please use `RegisterInstance` or `Use` method instead")]
    public void UseInstance(Type serviceType, object instance, IfAlreadyRegistered ifAlreadyRegistered,
        bool preventDisposal, bool weaklyReferenced, object serviceKey)
    {
      ThrowIfContainerDisposed();

      if (instance != null)
        instance.ThrowIfNotInstanceOf(serviceType, Error.RegisteringInstanceNotAssignableToServiceType);

      if (weaklyReferenced)
        instance = new WeakReference(instance);
      else if (preventDisposal)
        instance = new HiddenDisposable(instance);

      var scope = _ownCurrentScope ?? _singletonScope;
      var reuse = scope == _singletonScope ? Reuse.Singleton : Reuse.Scoped;
      var instanceType = instance?.GetType() ?? typeof(object);

      _registry.Swap(r =>
      {
        var entry = r.Services.GetValueOrDefault(serviceType);
        var oldEntry = entry;

        // no entries, first registration, usual/hot path
        if (entry == null)
        {
          // add new entry with instance factory
          var instanceFactory = new InstanceFactory(instance, instanceType, reuse, scope);
          entry = serviceKey == null
              ? (object)instanceFactory
              : FactoriesEntry.Empty.With(instanceFactory, serviceKey);
        }
        else
        {
          // have some registrations of instance, find if we should replace, add, or throw
          var singleDefaultFactory = entry as Factory;
          if (singleDefaultFactory != null)
          {
            if (serviceKey != null)
            {
              // @ifAlreadyRegistered does not make sense for keyed, because there are no other keyed
              entry = FactoriesEntry.Empty.With(singleDefaultFactory)
                  .With(new InstanceFactory(instance, instanceType, reuse, scope), serviceKey);
            }
            else // for default instance
            {
              switch (ifAlreadyRegistered)
              {
                case IfAlreadyRegistered.AppendNotKeyed:
                  entry = FactoriesEntry.Empty.With(singleDefaultFactory)
                      .With(new InstanceFactory(instance, instanceType, reuse, scope));
                  break;
                case IfAlreadyRegistered.Throw:
                  Throw.It(Error.UnableToRegisterDuplicateDefault, serviceType, singleDefaultFactory);
                  break;
                case IfAlreadyRegistered.Keep:
                  break;
                case IfAlreadyRegistered.Replace:
                  var reusedFactory = singleDefaultFactory as InstanceFactory;
                  if (reusedFactory != null)
                    scope.SetOrAdd(reusedFactory.FactoryID, instance);
                  else if (reuse != Reuse.Scoped) // for non-instance single registration, just replace with non-scoped instance only
                    entry = new InstanceFactory(instance, instanceType, reuse, scope);
                  else
                    entry = FactoriesEntry.Empty.With(singleDefaultFactory)
                        .With(new InstanceFactory(instance, instanceType, reuse, scope));
                  break;
                case IfAlreadyRegistered.AppendNewImplementation: // otherwise Keep the old one
                  if (singleDefaultFactory.CanAccessImplementationType &&
                      singleDefaultFactory.ImplementationType != instanceType)
                    entry = FactoriesEntry.Empty.With(singleDefaultFactory)
                        .With(new InstanceFactory(instance, instanceType, reuse, scope));
                  break;
              }
            }
          }
          else // for multiple existing or single keyed factory
          {
            var singleKeyedOrManyDefaultFactories = (FactoriesEntry)entry;
            if (serviceKey != null)
            {
              var singleKeyedFactory = singleKeyedOrManyDefaultFactories.Factories.GetValueOrDefault(serviceKey);
              if (singleKeyedFactory == null)
              {
                entry = singleKeyedOrManyDefaultFactories
                    .With(new InstanceFactory(instance, instanceType, reuse, scope), serviceKey);
              }
              else // when keyed instance is found
              {
                switch (ifAlreadyRegistered)
                {
                  case IfAlreadyRegistered.Replace:
                    var reusedFactory = singleKeyedFactory as InstanceFactory;
                    if (reusedFactory != null)
                      scope.SetOrAdd(reusedFactory.FactoryID, instance);
                    else
                      entry = singleKeyedOrManyDefaultFactories
                          .With(new InstanceFactory(instance, instanceType, reuse, scope), serviceKey);
                    break;
                  case IfAlreadyRegistered.Keep:
                    break;
                  default:
                    Throw.It(Error.UnableToRegisterDuplicateKey, serviceType, serviceKey, singleKeyedFactory);
                    break;
                }
              }
            }
            else // for default instance
            {
              var defaultFactories = singleKeyedOrManyDefaultFactories.LastDefaultKey == null
                  ? Empty<Factory>()
                  : singleKeyedOrManyDefaultFactories.Factories.Enumerate()
                      .Match(it => it.Key is DefaultKey, it => it.Value)
                      .ToArrayOrSelf();

              if (defaultFactories.Length == 0) // no default factories among the multiple existing keyed factories
              {
                entry = singleKeyedOrManyDefaultFactories
                    .With(new InstanceFactory(instance, instanceType, reuse, scope));
              }
              else // there are existing default factories
              {
                switch (ifAlreadyRegistered)
                {
                  case IfAlreadyRegistered.AppendNotKeyed:
                    entry = singleKeyedOrManyDefaultFactories
                        .With(new InstanceFactory(instance, instanceType, reuse, scope));
                    break;
                  case IfAlreadyRegistered.Throw:
                    Throw.It(Error.UnableToRegisterDuplicateDefault, serviceType, defaultFactories);
                    break;
                  case IfAlreadyRegistered.Keep:
                    break; // entry does not change
                  case IfAlreadyRegistered.Replace:
                    var instanceFactories = defaultFactories.Match(f => f is InstanceFactory);
                    if (instanceFactories.Length == 1)
                    {
                      scope.SetOrAdd(instanceFactories[0].FactoryID, instance);
                    }
                    else // multiple default or a keyed factory
                    {
                      // scoped instance may be appended only, and not replacing anything
                      if (reuse == Reuse.Scoped)
                      {
                        entry = singleKeyedOrManyDefaultFactories
                            .With(new InstanceFactory(instance, instanceType, reuse, scope));
                      }
                      else // here is the replacement goes on
                      {
                        var keyedFactories = singleKeyedOrManyDefaultFactories.Factories.Enumerate()
                            .Match(it => !(it.Key is DefaultKey)).ToArrayOrSelf();

                        if (keyedFactories.Length == 0) // replaces all default factories?
                          entry = new InstanceFactory(instance, instanceType, reuse, scope);
                        else
                        {
                          var factoriesEntry = FactoriesEntry.Empty;
                          for (var i = 0; i < keyedFactories.Length; i++)
                            factoriesEntry = factoriesEntry
                                .With(keyedFactories[i].Value, keyedFactories[i].Key);
                          entry = factoriesEntry
                              .With(new InstanceFactory(instance, instanceType, reuse, scope));
                        }
                      }
                    }
                    break;
                  case IfAlreadyRegistered.AppendNewImplementation: // otherwise Keep the old one
                    var duplicateImplIndex = defaultFactories.IndexOf(
                        x => x.CanAccessImplementationType && x.ImplementationType == instanceType);
                    if (duplicateImplIndex == -1) // add new implementation
                      entry = singleKeyedOrManyDefaultFactories
                          .With(new InstanceFactory(instance, instanceType, reuse, scope));
                    // otherwise do nothing - keep the old entry
                    break;
                }
              }
            }
          }
        }

        var hash = RuntimeHelpers.GetHashCode(serviceType);
        var registry = r.WithServices(r.Services.AddOrUpdate(hash, serviceType, entry));

        // clearing the resolution cache for the updated factory if any
        if (oldEntry != null && oldEntry != entry)
        {
          var oldFactory = oldEntry as Factory;
          if (oldFactory != null)
            registry.DropFactoryCache(oldFactory, hash, serviceType);
          else
            ((FactoriesEntry)oldEntry).Factories.Enumerate().ToArray()
                .ForEach(x => registry.DropFactoryCache(x.Value, hash, serviceType, serviceKey));
        }

        return registry;
      });
    }

    void IResolverContext.UseInstance(Type serviceType, object instance, IfAlreadyRegistered ifAlreadyRegistered,
        bool preventDisposal, bool weaklyReferenced, object serviceKey) =>
        UseInstance(serviceType, instance, ifAlreadyRegistered, preventDisposal, weaklyReferenced, serviceKey);

    void IRegistrator.UseInstance(Type serviceType, object instance, IfAlreadyRegistered ifAlreadyRegistered,
        bool preventDisposal, bool weaklyReferenced, object serviceKey) =>
        UseInstance(serviceType, instance, ifAlreadyRegistered, preventDisposal, weaklyReferenced, serviceKey);

    void IResolverContext.InjectPropertiesAndFields(object instance, string[] propertyAndFieldNames)
    {
      var instanceType = instance.ThrowIfNull().GetType();

      PropertiesAndFieldsSelector propertiesAndFields = null;
      if (!propertyAndFieldNames.IsNullOrEmpty())
      {
        var matchedMembers = instanceType.GetTypeInfo().DeclaredMembers.Match(
            m => (m is PropertyInfo || m is FieldInfo) && propertyAndFieldNames.IndexOf(m.Name) != -1,
            PropertyOrFieldServiceInfo.Of);
        // todo: Should we throw when no props are found?
        propertiesAndFields = matchedMembers.ToFunc<Request, IEnumerable<PropertyOrFieldServiceInfo>>;
      }

      propertiesAndFields = propertiesAndFields ?? Rules.PropertiesAndFields ?? PropertiesAndFields.Auto;

      var request = Request.Create(this, instanceType)
          .WithResolvedFactory(new RegisteredInstanceFactory(instance, Reuse.Transient),
              skipRecursiveDependencyCheck: true, skipCaptiveDependencyCheck: true);

      foreach (var serviceInfo in propertiesAndFields(request))
        if (serviceInfo != null)
        {
          var details = serviceInfo.Details;
          var value = ((IResolver)this).Resolve(serviceInfo.ServiceType, details.ServiceKey,
              details.IfUnresolved, details.RequiredServiceType, request, args: null);
          if (value != null)
            serviceInfo.SetValue(instance, value);
        }
    }

    /// Adding the factory directly to scope for resolution 
    public void Use(Type serviceType, FactoryDelegate factory)
    {
      var typeHash = RuntimeHelpers.GetHashCode(serviceType);
      (CurrentScope ?? SingletonScope).SetUsedInstance(typeHash, serviceType, factory);
      var cacheEntry = _registry.Value.GetCachedDefaultFactoryOrDefault(typeHash, serviceType);
      if (cacheEntry != null)
        cacheEntry.Value.Value = null;
    }

    #endregion

    #region IContainer

    /// <summary>The rules object defines policies per container for registration and resolution.</summary>
    public Rules Rules { get; private set; }

    /// <summary>Represents scope bound to container itself, and not the ambient (context) thing.</summary>
    public IScope OwnCurrentScope => _ownCurrentScope;

    /// <summary>Indicates that container is disposed.</summary>
    public bool IsDisposed => _disposed == 1 || _singletonScope.IsDisposed;

    /// <inheritdoc />
    public IContainer With(Rules rules, IScopeContext scopeContext, RegistrySharing registrySharing, IScope singletonScope) =>
        With(_parent, rules, scopeContext, registrySharing, singletonScope, _ownCurrentScope, null);

    /// <inheritdoc />
    public IContainer With(IResolverContext parent, Rules rules, IScopeContext scopeContext,
        RegistrySharing registrySharing, IScope singletonScope, IScope currentScope) =>
        With(_parent, rules, scopeContext, registrySharing, singletonScope, currentScope, null);

    /// <inheritdoc />
    public IContainer With(IResolverContext parent, Rules rules, IScopeContext scopeContext,
        RegistrySharing registrySharing, IScope singletonScope, IScope currentScope,
        IsRegistryChangePermitted? isRegistryChangePermitted)
    {
      ThrowIfContainerDisposed();

      var registry =
          registrySharing == RegistrySharing.Share ?
              _registry :
          registrySharing == RegistrySharing.CloneButKeepCache
              ? Ref.Of(_registry.Value)
              : Ref.Of(_registry.Value.WithoutCache());

      if (isRegistryChangePermitted != null &&
          isRegistryChangePermitted.Value != registry.Value.IsChangePermitted)
        registry = Ref.Of(registry.Value.WithIsChangePermitted(isRegistryChangePermitted.Value));

      return new Container(rules ?? Rules, registry, singletonScope ?? NewSingletonScope(), scopeContext,
          currentScope ?? _ownCurrentScope, _disposed, _disposeStackTrace, parent ?? _parent);
    }

    /// <summary>Produces new container which prevents any further registrations.</summary>
    /// <param name="ignoreInsteadOfThrow">(optional) Controls what to do with the next registration: ignore or throw exception. Throws exception by default.</param>
    public IContainer WithNoMoreRegistrationAllowed(bool ignoreInsteadOfThrow = false) =>
        With(_parent, Rules, _scopeContext, RegistrySharing.Share, _singletonScope, _ownCurrentScope,
            ignoreInsteadOfThrow ? IsRegistryChangePermitted.Ignored : IsRegistryChangePermitted.Error);

    /// <inheritdoc />
    public bool ClearCache(Type serviceType, FactoryType? factoryType, object serviceKey)
    {
      var hash = RuntimeHelpers.GetHashCode(serviceType);

      if (factoryType != null)
        return _registry.Value.ClearCache(hash, serviceType, serviceKey, factoryType.Value);

      var registry = _registry.Value;

      var clearedServices = registry.ClearCache(hash, serviceType, serviceKey, FactoryType.Service);
      var clearedWrapper = registry.ClearCache(hash, serviceType, serviceKey, FactoryType.Wrapper);
      var clearedDecorator = registry.ClearCache(hash, serviceType, serviceKey, FactoryType.Decorator);

      return clearedServices || clearedWrapper || clearedDecorator;
    }

    [MethodImpl((MethodImplOptions)256)]
    internal Expression GetCachedFactoryExpression(int factoryId, IReuse reuse, out ImMapEntry<Registry.ExpressionCacheSlot> slot) =>
        _registry.Value.GetCachedFactoryExpression(factoryId, reuse, out slot);

    [MethodImpl((MethodImplOptions)256)]
    internal void CacheFactoryExpression(int factoryId, Expression expr, IReuse reuse, int dependencyCount,
        ImMapEntry<Registry.ExpressionCacheSlot> slot) =>
        _registry.Value.CacheFactoryExpression(factoryId, expr, reuse, dependencyCount, slot);

    Factory IContainer.ResolveFactory(Request request)
    {
      var factory = ((IContainer)this).GetServiceFactoryOrDefault(request);
      if (factory == null)
      {
        factory = GetWrapperFactoryOrDefault(request);
        if (factory != null)
          return factory;

        var unknownServiceResolvers = Rules.UnknownServiceResolvers;
        if (!unknownServiceResolvers.IsNullOrEmpty())
          for (var i = 0; factory == null && i < unknownServiceResolvers.Length; i++)
            factory = unknownServiceResolvers[i](request)?.DoNotCache();
      }

      if (factory?.FactoryGenerator != null)
        factory = factory.FactoryGenerator.GetGeneratedFactory(request);

      if (factory == null)
        TryThrowUnableToResolve(request);

      return factory;
    }

    internal static void TryThrowUnableToResolve(Request request)
    {
      if (request.IfUnresolved != IfUnresolved.Throw)
        return;

      var str = new StringBuilder();
      str = request.Container
          .GetAllServiceFactories(request.ServiceType, bothClosedAndOpenGenerics: true)
          .Aggregate(str, (s, x) => s
              .Append((x.Value.Reuse?.CanApply(request) ?? true) ? "  " : "  without matching scope ")
              .Print(x));

      if (str.Length != 0)
        Throw.It(Error.UnableToResolveFromRegisteredServices, request, str);
      else
        Throw.It(Error.UnableToResolveUnknownService, request,
            request.Rules.DynamicRegistrationProviders.EmptyIfNull().Length,
            request.Rules.UnknownServiceResolvers.EmptyIfNull().Length);
    }

    Factory IContainer.GetServiceFactoryOrDefault(Request request)
    {
      Type serviceType;
      var serviceKey = request.ServiceKey;
      var requiredServiceType = request.RequiredServiceType;
      if (requiredServiceType != null && requiredServiceType.IsOpenGeneric())
        serviceType = requiredServiceType;
      else
      {
        serviceType = request.GetActualServiceType();

        // Special case when open-generic required service type is encoded in ServiceKey as array of { ReqOpenGenServiceType, ServiceKey }
        // presumes that required service type is closed generic
        if (serviceKey is OpenGenericTypeKey openGenericTypeKey &&
            serviceType.IsClosedGeneric() &&
            openGenericTypeKey.RequiredServiceType == serviceType.GetGenericTypeDefinition())
        {
          serviceType = openGenericTypeKey.RequiredServiceType;
          serviceKey = openGenericTypeKey.ServiceKey;
        }
      }

      if (Rules.FactorySelector != null && serviceKey == null)
        return GetRuleSelectedServiceFactoryOrDefault(request, serviceType);

      var serviceFactories = _registry.Value.Services;
      var entry = serviceFactories.GetValueOrDefault(serviceType);

      // For closed-generic type, when the entry is not found or the key in entry is not found go for the open-generic services
      var openGenericServiceType = serviceType.IsClosedGeneric() ? serviceType.GetGenericTypeDefinition() : null;
      if (openGenericServiceType != null)
      {
        if (entry == null ||
            serviceKey != null && (
            entry is Factory && !serviceKey.Equals(DefaultKey.Value) ||
            entry is FactoriesEntry factoriesEntry && factoriesEntry.Factories.GetValueOrDefault(serviceKey) == null))
          entry = serviceFactories.GetValueOrDefault(openGenericServiceType) ?? entry;

        if (entry == null && Rules.VariantGenericTypesInResolve)
        {
          foreach (var e in serviceFactories.Enumerate())
          {
            if (e.Value.Value is Factory f)
            {
              if ((serviceKey == null || serviceKey == DefaultKey.Value) &&
                  serviceType.IsAssignableVariantGenericTypeFrom(e.Value.Key) &&
                  request.MatchFactoryConditionAndMetadata(f))
              {
                entry = f;
                break;
              }
            }
            else
            {
              foreach (var kf in ((FactoriesEntry)e.Value.Value).Factories.Enumerate())
                if (kf.Key.Equals(serviceKey) &&
                    serviceType.IsAssignableVariantGenericTypeFrom(e.Value.Key) &&
                    request.MatchFactoryConditionAndMetadata(kf.Value))
                {
                  entry = kf.Value;
                  break;
                }
            }
          }
        }
      }

      // Most common case when we have a single default factory and no dynamic rules to always apply
      if (entry is Factory singleDefaultFactory &&
          (Rules.DynamicRegistrationProviders == null ||
          !Rules.HasDynamicRegistrationProvider(DynamicRegistrationFlags.Service, withoutFlags: DynamicRegistrationFlags.AsFallback)))
      {
        if (serviceKey != null && serviceKey != DefaultKey.Value ||
            !singleDefaultFactory.CheckCondition(request) ||
            (request.MetadataKey != null || request.Metadata != null) &&
            !singleDefaultFactory.Setup.MatchesMetadata(request.MetadataKey, request.Metadata))
          return null;
        return singleDefaultFactory;
      }

      var factories = entry == null ? Empty<KV<object, Factory>>()
          : entry is Factory factory ? new KV<object, Factory>(DefaultKey.Value, factory).One()
          : entry.To<FactoriesEntry>().Factories
              .Visit(new List<KV<object, Factory>>(2), (x, list) => list.Add(KV.Of(x.Key, x.Value))).ToArray() // todo: optimize - we may not need ToArray here
              .Match(x => x.Value != null); // filter out the Unregistered factories (see #390)

      if (Rules.DynamicRegistrationProviders != null &&
          !serviceType.IsExcludedGeneralPurposeServiceType() &&
          !((IContainer)this).IsWrapper(serviceType))
        factories = CombineRegisteredServiceWithDynamicFactories(factories, serviceType, openGenericServiceType, serviceKey);

      if (factories.Length == 0)
        return null;

      // For requested keyed service (which may be a `DefaultKey` or `DefaultDynamicKey`)
      // just lookup for the key and return whatever the result
      if (serviceKey != null)
      {
        foreach (var f in factories)
          if (serviceKey.Equals(f.Key) && f.Value.CheckCondition(request))
            return f.Value;
        return null;
      }

      // First, filter out non default normal and dynamic factories
      factories = factories.Match(f => f.Key is DefaultKey || f.Key is DefaultDynamicKey);

      var defaultFactoriesCount = factories.Length;
      if (defaultFactoriesCount == 0)
        return null;

      // For multiple matched factories if the single one has a condition, then use it
      factories = factories.Match(request, (r, x) => r.MatchFactoryConditionAndMetadata(x.Value));

      // Check the for the reuse matching scopes (for a single the check will be down the road) (BBIssue: #175)
      if (factories.Length > 1 && Rules.ImplicitCheckForReuseMatchingScope)
      {
        KV<object, Factory> singleMatchedFactory = null;
        var reuseMatchedFactories = factories.Match(request, (r, x) => r.MatchFactoryReuse(x.Value));
        if (reuseMatchedFactories.Length == 1)
          singleMatchedFactory = reuseMatchedFactories[0];
        else if (reuseMatchedFactories.Length > 1)
          singleMatchedFactory = FindFactoryWithTheMinReuseLifespanOrDefault(factories);

        if (singleMatchedFactory != null)
        {
          // Add asResolutionCall or change the serviceKey to prevent the caching of expression as default (BBIssue: #382)
          if (!request.IsResolutionCall)
            singleMatchedFactory.Value.Setup = singleMatchedFactory.Value.Setup.WithAsResolutionCall();
          else
            request.ChangeServiceKey(singleMatchedFactory.Key);
          return singleMatchedFactory.Value; // we are done
        }
      }

      // Match open-generic implementation with closed service type. Performance is OK because the generated factories are cached -
      // so there should not be repeating of the check, and not match of Performance decrease.
      if (factories.Length > 1)
        factories = factories.Match(request, (r, x) =>
            x.Value.FactoryGenerator == null || x.Value.FactoryGenerator.GetGeneratedFactory(r, ifErrorReturnDefault: true) != null);

      if (factories.Length > 1)
      {
        // prefer the factories with the condition (they should be evaluated / matched earlier anyway)
        var conditionedFactories = factories.Match(f => f.Value.Setup.Condition != null);
        if (conditionedFactories.Length == 1)
          factories = conditionedFactories;
      }

      if (factories.Length > 1)
      {
        // prefer the factories with the `Setup.PreferInSingleServiceResolve`
        var preferredFactories = factories.Match(f => f.Value.Setup.PreferInSingleServiceResolve);
        if (preferredFactories.Length == 1)
          factories = preferredFactories;
      }

      // The result is a single matched factory
      if (factories.Length == 1)
      {
        // Changes service key for resolution call to identify single factory in cache and prevent wrong hit
        if (defaultFactoriesCount > 1 && request.IsResolutionCall)
          request.ChangeServiceKey(factories[0].Key);
        return factories[0].Value;
      }

      if (factories.Length > 1 && request.IfUnresolved == IfUnresolved.Throw)
        Throw.It(Error.ExpectedSingleDefaultFactory, factories, request);

      // Return null to allow fallback strategies
      return null;
    }

    private Factory GetRuleSelectedServiceFactoryOrDefault(Request request, Type serviceType)
    {
      var serviceFactories = _registry.Value.Services;
      var entry = serviceFactories.GetValueOrDefault(serviceType);

      var openGenericServiceType = serviceType.GetGenericDefinitionOrNull();
      KV<object, Factory>[] factories;
      if (entry is Factory singleDefaultFactory)
      {
        if (Rules.DynamicRegistrationProviders == null ||
            !Rules.HasDynamicRegistrationProvider(DynamicRegistrationFlags.Service, withoutFlags: DynamicRegistrationFlags.AsFallback))
          return request.MatchFactoryConditionAndMetadata(singleDefaultFactory)
              ? Rules.FactorySelector(request, DefaultKey.Value.Pair<object, Factory>(singleDefaultFactory).One())
              : null;

        factories = new[] { new KV<object, Factory>(DefaultKey.Value, singleDefaultFactory) };
      }
      else if (entry is FactoriesEntry e)
      {
        factories = e.Factories.Visit(new List<KV<object, Factory>>(), (x, l) => l.Add(KV.Of(x.Key, x.Value))).ToArray();
      }
      else
      {
        object openGenericEntry;
        factories = Empty<KV<object, Factory>>();
        if (openGenericServiceType != null)
        {
          openGenericEntry = serviceFactories.GetValueOrDefault(openGenericServiceType);
          if (openGenericEntry != null)
            factories = openGenericEntry is Factory f ? new[] { new KV<object, Factory>(DefaultKey.Value, f) } :
                openGenericEntry.To<FactoriesEntry>().Factories
                    .Visit(new List<KV<object, Factory>>(), (x, l) => l.Add(KV.Of(x.Key, x.Value))).ToArray()
                    .Match(x => x.Value != null);

          if (openGenericEntry == null && Rules.VariantGenericTypesInResolve)
          {
            foreach (var sf in serviceFactories.Enumerate())
            {
              if (sf.Value.Value is Factory f)
              {
                if (serviceType.IsAssignableVariantGenericTypeFrom(sf.Value.Key) &&
                    request.MatchFactoryConditionAndMetadata(f))
                {
                  factories = KV.Of<object, Factory>(DefaultKey.Value, f).One();
                  break;
                }
              }
              else
              {
                foreach (var kf in ((FactoriesEntry)sf.Value.Value).Factories.Enumerate())
                  if (serviceType.IsAssignableVariantGenericTypeFrom(sf.Value.Key) &&
                      request.MatchFactoryConditionAndMetadata(kf.Value))
                  {
                    factories = KV.Of(kf.Key, kf.Value).One();
                    break;
                  }
              }
            }
          }
        }
      }

      if (Rules.DynamicRegistrationProviders != null &&
          !serviceType.IsExcludedGeneralPurposeServiceType() &&
          !((IContainer)this).IsWrapper(serviceType))
        factories = CombineRegisteredServiceWithDynamicFactories(factories, serviceType, openGenericServiceType);

      if (factories.Length == 0)
        return null;

      // optimize for the case with the single factory
      if (factories.Length == 1)
        return request.MatchFactoryConditionAndMetadata(factories[0].Value)
            ? Rules.FactorySelector(request, factories[0].Key.Pair(factories[0].Value).One())
            : null;

      // Sort in registration order
      if (factories.Length > 1)
        Array.Sort(factories, _lastFactoryIDWinsComparer);

      var matchedFactories = factories.Match(request, (r, x) => r.MatchFactoryConditionAndMetadata(x.Value));
      if (matchedFactories.Length > 1 && Rules.ImplicitCheckForReuseMatchingScope)
      {
        // Check for the matching scopes. Only for more than one factory, 
        // for the single factory the check will be down the road (BBIssue #175)
        matchedFactories = matchedFactories.Match(request, (r, x) => r.MatchFactoryReuse(x.Value));
        // Add asResolutionCall for the factory to prevent caching of in-lined expression in context with not matching condition (BBIssue #382)
        if (matchedFactories.Length == 1 && !request.IsResolutionCall)
          matchedFactories[0].Value.Setup = matchedFactories[0].Value.Setup.WithAsResolutionCall();
      }

      // Match open-generic implementation with closed service type. Performance is OK because the generated factories are cached -
      // so there should not be repeating of the check, and not match of Performance decrease.
      if (matchedFactories.Length > 1)
        matchedFactories = matchedFactories.Match(request,
            (r, x) => x.Value.FactoryGenerator == null || x.Value.FactoryGenerator.GetGeneratedFactory(r, ifErrorReturnDefault: true) != null);

      if (matchedFactories.Length == 0)
        return null;

      var selectedFactory = Rules.FactorySelector(request, matchedFactories.Map(x => x.Key.Pair(x.Value)));
      if (selectedFactory == null)
        return null;

      // BBIssue: #508, GHIssue: #350
      if (request.IsResolutionCall && factories.Length > 1)
      {
        var i = 0;
        while (i < matchedFactories.Length && matchedFactories[i].Value.FactoryID != selectedFactory.FactoryID)
          ++i;
        if (i < matchedFactories.Length)
          request.ChangeServiceKey(matchedFactories[i].Key);
      }

      return selectedFactory;
    }

    // Don't forget that we have the same public method Rules.SelectFactoryWithTheMinReuseLifespan
    private static KV<object, Factory> FindFactoryWithTheMinReuseLifespanOrDefault(KV<object, Factory>[] factories)
    {
      var minLifespan = int.MaxValue;
      var multipleFactories = false;
      KV<object, Factory> minLifespanFactory = null;

      foreach (var factory in factories)
      {
        var reuse = factory.Value.Reuse;
        var lifespan = reuse == null || reuse == Reuse.Transient ? int.MaxValue : reuse.Lifespan;
        if (lifespan == minLifespan)
          multipleFactories = true;
        else if (lifespan < minLifespan)
        {
          minLifespan = lifespan;
          minLifespanFactory = factory;
          multipleFactories = false;
        }
      }

      return !multipleFactories && minLifespanFactory != null ? minLifespanFactory : null;
    }

    IEnumerable<KV<object, Factory>> IContainer.GetAllServiceFactories(Type serviceType, bool bothClosedAndOpenGenerics)
    {
      var registry = _registry.Value;
      var serviceFactories = registry.Services;
      var entry = serviceFactories.GetValueOrDefault(serviceType);

      var factories = entry == null ? Empty<KV<object, Factory>>()
          : entry is Factory f ? new[] { new KV<object, Factory>(DefaultKey.Value, f) }
          : entry.To<FactoriesEntry>().Factories
              .Visit(new List<KV<object, Factory>>(), (x, l) => l.Add(KV.Of(x.Key, x.Value))).ToArray()
              .Match(x => x.Value != null); // filter out the Unregistered factories

      var openGenericServiceType = bothClosedAndOpenGenerics && serviceType.IsClosedGeneric() ? serviceType.GetGenericTypeDefinition() : null;
      if (openGenericServiceType != null)
      {
        var openGenericEntry = serviceFactories.GetValueOrDefault(openGenericServiceType);
        if (openGenericEntry != null)
          factories = openGenericEntry is Factory gf
              ? factories.Append(new KV<object, Factory>(DefaultKey.Value, gf))
              : factories.Append(((FactoriesEntry)openGenericEntry).Factories
                  .Visit(new List<KV<object, Factory>>(), (x, l) => l.Add(KV.Of(x.Key, x.Value))).ToArray()
                  .Match(x => x.Value != null)); // filter out the Unregistered factories
      }

      if (Rules.DynamicRegistrationProviders != null &&
          !serviceType.IsExcludedGeneralPurposeServiceType())
        return CombineRegisteredServiceWithDynamicFactories(factories, serviceType, openGenericServiceType);

      return factories;
    }

    KV<object, Factory>[] IContainer.GetServiceRegisteredAndDynamicFactories(Type serviceType)
    {
      var serviceFactories = _registry.Value.Services;
      var entry = serviceFactories.GetValueOrDefault(serviceType);

      var factories = entry == null ? Empty<KV<object, Factory>>()
          : entry is Factory f ? new[] { new KV<object, Factory>(DefaultKey.Value, f) }
          : entry.To<FactoriesEntry>().Factories
              .Visit(new List<KV<object, Factory>>(), (x, l) => l.Add(KV.Of(x.Key, x.Value))).ToArray()
              .Match(x => x.Value != null); // filter out the Unregistered factories

      if (Rules.DynamicRegistrationProviders != null &&
          !serviceType.IsExcludedGeneralPurposeServiceType())
        return CombineRegisteredServiceWithDynamicFactories(factories, serviceType, null);

      return factories;
    }

    private static int _objectTypeHash = RuntimeHelpers.GetHashCode(typeof(object));

    Expression IContainer.GetDecoratorExpressionOrDefault(Request request)
    {
      var container = request.Container;
      // return early if no decorators registered
      if (_registry.Value.Decorators.IsEmpty &&
          (container.Rules.DynamicRegistrationProviders == null ||
          !container.Rules.HasDynamicRegistrationProvider(DynamicRegistrationFlags.Decorator))) // todo: @perf reuse its result
        return null;

      var arrayElementType = request.ServiceType.GetArrayElementTypeOrNull();
      if (arrayElementType != null)
        request = request.WithChangedServiceInfo(x => // todo: @perf optimize allocations
            x.With(typeof(IEnumerable<>).MakeGenericType(arrayElementType)));

      var serviceType = request.ServiceType;
      var decorators = container.GetDecoratorFactoriesOrDefault(serviceType);

      // Combine with required service type if different from service type
      var requiredServiceType = request.GetActualServiceType();
      if (requiredServiceType != serviceType)
        decorators = decorators.Append(container.GetDecoratorFactoriesOrDefault(requiredServiceType));

      // Define the list of ids for the already applied decorators
      int[] appliedDecoratorIDs = null;
      if (!decorators.IsNullOrEmpty()) // todo: @perf check earlier for `p.DirectParent.IsEmpty && p.DirectParent.FactoryType != FactoryType.Service` to avoid method calling and check inside
      {
        appliedDecoratorIDs = GetAppliedDecoratorIDs(request);
        if (appliedDecoratorIDs.Length != 0)
          decorators = decorators.Match(appliedDecoratorIDs, (ids, d) =>
          {
            var id = d.FactoryID;
            for (var i = 0; i < ids.Length; ++i)
              if (id == ids[i])
                return false;
            return true;
          });
      }

      // Append open-generic decorators
      var genericDecorators = Empty<Factory>();
      var openGenericServiceType = serviceType.GetGenericDefinitionOrNull();
      if (openGenericServiceType != null)
        genericDecorators = container.GetDecoratorFactoriesOrDefault(openGenericServiceType);

      // Combine with open-generic required type if they are different from service type
      if (requiredServiceType != serviceType)
      {
        var openGenericRequiredType = requiredServiceType.GetGenericDefinitionOrNull();
        if (openGenericRequiredType != null && openGenericRequiredType != openGenericServiceType)
          genericDecorators = genericDecorators.Append(
              container.GetDecoratorFactoriesOrDefault(openGenericRequiredType));
      }

      // Append generic type argument decorators, registered as Object
      // Note: the condition for type arguments should be checked before generating the closed generic version
      // Note: the dynamic rules for the object is not supported, sorry - to much of performance hog to be called every time
      var typeArgDecorators = _registry.Value.Decorators.GetValueOrDefault(_objectTypeHash, typeof(object)) as Factory[];
      if (!typeArgDecorators.IsNullOrEmpty())
      {
        typeArgDecorators = typeArgDecorators.Match(request, (r, d) => d.CheckCondition(r));
        if (typeArgDecorators.Length > 0)
          genericDecorators = genericDecorators.Append(typeArgDecorators);
      }

      // Filter out already applied generic decorators
      // And combine with rest of decorators
      if (!genericDecorators.IsNullOrEmpty())
      {
        appliedDecoratorIDs = appliedDecoratorIDs ?? GetAppliedDecoratorIDs(request);
        if (!appliedDecoratorIDs.IsNullOrEmpty())
        {
          genericDecorators = genericDecorators.Match(appliedDecoratorIDs,
              (appliedDecIds, d) =>
              {
                var factoryGenerator = d.FactoryGenerator;
                if (factoryGenerator == null)
                  return appliedDecIds.IndexOf(d.FactoryID) == -1;

                foreach (var entry in factoryGenerator.GeneratedFactories.Enumerate())
                  if (appliedDecIds.IndexOf(entry.Value.FactoryID) != -1)
                    return false;

                return true;
              });
        }

        // Generate closed-generic versions
        if (!genericDecorators.IsNullOrEmpty())
        {
          genericDecorators = genericDecorators
              .Map(request, (r, d) => d.FactoryGenerator == null ? d : d.FactoryGenerator.GetGeneratedFactory(r, ifErrorReturnDefault: true))
              .Match(d => d != null);
          decorators = decorators.Append(genericDecorators);
        }
      }

      // Filter out the recursive decorators by doing the same recursive check
      // that Request.WithResolvedFactory does. Fixes: #267
      if (!decorators.IsNullOrEmpty())
        decorators = decorators.Match(request, (r, d) => !r.HasRecursiveParent(d.FactoryID));

      // Return earlier if no decorators found, or we have filtered out everything
      if (decorators.IsNullOrEmpty())
        return null;

      Factory decorator = null;
      if (decorators.Length == 1)
      {
        decorator = decorators[0];
        if (!decorator.CheckCondition(request))
          return null;
      }
      else if (decorators.Length == 2)
      {
        var d0 = decorators[0];
        var d0Order = ((Setup.DecoratorSetup)d0.Setup).Order;
        var d1 = decorators[1];
        var d1Order = ((Setup.DecoratorSetup)d1.Setup).Order;
        if (d1Order > d0Order || d1Order == d0Order && d1.RegistrationOrder > d0.RegistrationOrder)
        {
          if (d1.CheckCondition(request))
            decorator = d1;
          else if (d0.CheckCondition(request))
            decorator = d0;
        }
        else
        {
          if (d0.CheckCondition(request))
            decorator = d0;
          else if (d1.CheckCondition(request))
            decorator = d1;
        }
      }
      else
      {
        // todo: maybe optimized for already sorted array to get rid off copy
        var sortedDecorators = SortBySetupOrderDescendingThenByRegistrationDescending(decorators.Copy());
        for (int i = sortedDecorators.Length - 1; decorator == null && i >= 0; i--)
          if (sortedDecorators[i].CheckCondition(request))
            decorator = sortedDecorators[i];
      }

      var decoratorExpr = decorator?.GetExpressionOrDefault(request);
      if (decoratorExpr == null)
        return null;

      // decorator of arrays should be converted back from IEnumerable to array.
      if (arrayElementType != null)
        decoratorExpr = Call(WrappersSupport.ToArrayMethod.MakeGenericMethod(arrayElementType), decoratorExpr);

      return decoratorExpr;
    }

    private static Factory[] SortBySetupOrderDescendingThenByRegistrationDescending(Factory[] ds)
    {
      int i, j;
      for (i = 1; i < ds.Length; ++i)
      {
        var d = ds[i];
        var order = ((Setup.DecoratorSetup)d.Setup).Order;
        j = i;
        while (j >= 1)
        {
          var prevOrder = ((Setup.DecoratorSetup)ds[j - 1].Setup).Order;
          if ((order < prevOrder ||
               order == prevOrder && d.RegistrationOrder < ds[j - 1].RegistrationOrder) == false)
            break;
          ds[j] = ds[j - 1];
          --j;
        }

        //if (ds[j] != d)
        ds[j] = d;
      }
      return ds;
    }

    private static int[] GetAppliedDecoratorIDs(Request request)
    {
      var requestFactoryID = request.FactoryID;
      var appliedIDs = Empty<int>();
      for (var p = request.DirectParent; !p.IsEmpty && p.FactoryType != FactoryType.Service; p = p.DirectParent)
        if (p.FactoryType == FactoryType.Decorator && p.DecoratedFactoryID == requestFactoryID)
          appliedIDs = appliedIDs.Append(p.FactoryID);
      return appliedIDs;
    }

    Factory IContainer.GetWrapperFactoryOrDefault(Type serviceType)
    {
      var wrappers = _registry.Value.Wrappers;
      var wrapper = wrappers.GetValueOrDefault(serviceType);
      if (wrapper == null)
      {
        serviceType = serviceType.GetGenericDefinitionOrNull();
        if (serviceType != null)
          wrapper = wrappers.GetValueOrDefault(serviceType);
      }
      return wrapper as Factory;
    }

    bool IContainer.IsWrapper(Type serviceType, Type openGenericServiceType) // todo: @perf optimize this
    {
      var wrappers = _registry.Value.Wrappers;
      return wrappers.GetValueOrDefault(serviceType) != null // todo: @todo reorder things to get faster results for the open-generic wrappers - for the rest perf won't change 
          || openGenericServiceType != null && wrappers.GetValueOrDefault(openGenericServiceType) != null;
    }

    // todo: @perf pass the serviceTypeHash
    Factory[] IContainer.GetDecoratorFactoriesOrDefault(Type serviceType)
    {
      var decorators = _registry.Value.Decorators.GetValueOrDefault(serviceType) as Factory[];

      if (Rules.DynamicRegistrationProviders != null)
        return CombineRegisteredDecoratorWithDynamicFactories(decorators, serviceType);

      return decorators;
    }

    Type IContainer.GetWrappedType(Type serviceType, Type requiredServiceType)
    {
      if (requiredServiceType != null && requiredServiceType.IsOpenGeneric())
        return ((IContainer)this).GetWrappedType(serviceType, null);

      serviceType = requiredServiceType ?? serviceType;

      var wrappedType = serviceType.GetArrayElementTypeOrNull();
      if (wrappedType == null)
      {
        var factory = ((IContainer)this).GetWrapperFactoryOrDefault(serviceType);
        if (factory != null)
        {
          wrappedType = ((Setup.WrapperSetup)factory.Setup).GetWrappedTypeOrNullIfWrapsRequired(serviceType);
          if (wrappedType == null)
            return null;
        }
      }

      return wrappedType == null ? serviceType
          : ((IContainer)this).GetWrappedType(wrappedType, null);
    }

    /// <summary>Converts known item into literal expression or wraps it in a constant expression.</summary>
    public Expression GetConstantExpression(object item, Type itemType = null, bool throwIfStateRequired = false)
    {
      // Check for UsedForExpressionGeneration, and if not set just short-circuit to Expression.Constant
      if (!throwIfStateRequired && !Rules.ThrowIfRuntimeStateRequired && !Rules.UsedForExpressionGeneration)
        return itemType == null ? Constant(item) : Constant(item, itemType);

      if (item == null)
        return itemType == null || itemType == typeof(object) ? Constant(null) : Constant(null, itemType);

      var convertible = item as IConvertibleToExpression;
      if (convertible != null)
        return convertible.ToExpression(it => GetConstantExpression(it, null, throwIfStateRequired));

      var actualItemType = item.GetType();
      if (actualItemType.GetGenericDefinitionOrNull() == typeof(KV<,>))
      {
        var kvArgTypes = actualItemType.GetGenericParamsAndArgs();
        return Call(_kvOfMethod.MakeGenericMethod(kvArgTypes),
            GetConstantExpression(actualItemType.GetTypeInfo().GetDeclaredField("Key").GetValue(item), kvArgTypes[0], throwIfStateRequired),
            GetConstantExpression(actualItemType.GetTypeInfo().GetDeclaredField("Value").GetValue(item), kvArgTypes[1], throwIfStateRequired));
      }

      if (actualItemType.IsPrimitive() ||
          actualItemType.IsAssignableTo<Type>())
        return itemType == null ? Constant(item) : Constant(item, itemType);

      // don't try to recover the non primitive type of element,
      // cause it is a too much work to find the base common element type in array
      var arrayElemType = actualItemType.GetArrayElementTypeOrNull();
      if (arrayElemType != null && arrayElemType != typeof(object) &&
         (arrayElemType.IsPrimitive() || actualItemType.IsAssignableTo<Type>()))
        return NewArrayInit(arrayElemType,
            ((object[])item).Map(x => GetConstantExpression(x, arrayElemType, throwIfStateRequired)));

      var itemExpr = Rules.ItemToExpressionConverter?.Invoke(item, itemType);
      if (itemExpr != null)
        return itemExpr;

      Throw.If(throwIfStateRequired || Rules.ThrowIfRuntimeStateRequired,
          Error.StateIsRequiredToUseItem, item);

      return itemType == null ? Constant(item) : Constant(item, itemType);
    }

    private static readonly MethodInfo _kvOfMethod =
        typeof(KV).GetTypeInfo().GetDeclaredMethod(nameof(KV.Of));

    #endregion

    #region Factories Add/Get

    internal sealed class FactoriesEntry
    {
      public readonly DefaultKey LastDefaultKey;
      public readonly ImHashMap<object, Factory> Factories;

      // lastDefaultKey may be null
      public FactoriesEntry(DefaultKey lastDefaultKey, ImHashMap<object, Factory> factories)
      {
        LastDefaultKey = lastDefaultKey;
        Factories = factories;
      }

      public static readonly FactoriesEntry Empty =
          new FactoriesEntry(null, ImHashMap<object, Factory>.Empty);

      public FactoriesEntry With(Factory factory)
      {
        var lastDefaultKey = LastDefaultKey == null ? DefaultKey.Value : LastDefaultKey.Next();
        return new FactoriesEntry(lastDefaultKey, Factories.AddOrUpdate(lastDefaultKey, factory));
      }

      public FactoriesEntry WithTwo(Factory oldFactory, Factory newFactory)
      {
        var lastDefaultKey = LastDefaultKey == null ? DefaultKey.Value : LastDefaultKey.Next();
        var factories = Factories
            .AddOrUpdate(lastDefaultKey, oldFactory)
            .AddOrUpdate(lastDefaultKey = lastDefaultKey.Next(), newFactory);
        return new FactoriesEntry(lastDefaultKey, factories);
      }

      public FactoriesEntry With(Factory factory, object serviceKey) =>
          new FactoriesEntry(LastDefaultKey, Factories.AddOrUpdate(serviceKey, factory));
    }

    private KV<object, Factory>[] CombineRegisteredServiceWithDynamicFactories(
        KV<object, Factory>[] factories, Type serviceType, Type openGenericServiceType, object serviceKey = null)
    {
      var withFlags = DynamicRegistrationFlags.Service;
      var withoutFlags = factories.Length != 0 ? DynamicRegistrationFlags.AsFallback : DynamicRegistrationFlags.NoFlags;

      // Assign unique continuous keys across all of dynamic providers,
      // to prevent duplicate keys and peeking the wrong factory by collection wrappers
      // NOTE: Given that dynamic registration always return the same implementation types in the same order
      // then the dynamic key will be assigned deterministically, so that even if `CombineRegisteredWithDynamicFactories`
      // is called multiple times during the resolution (like for `ResolveMany`) it is possible to match the required factory by its order.
      DefaultDynamicKey dynamicKey = null;

      var dynamicFlags = Rules.DynamicRegistrationFlags;
      for (var i = 0; i < dynamicFlags.Length; ++i)
      {
        var flag = dynamicFlags[i];
        if ((flag & withFlags) != withFlags || (flag & withoutFlags) != 0)
          continue;

        var dynamicRegistrationProvider = Rules.DynamicRegistrationProviders[i];
        var dynamicRegistrations = dynamicRegistrationProvider(serviceType, serviceKey).ToArrayOrSelf();

      restartWithOpenGenericRegistrations:
        if (dynamicRegistrations.Length != 0)
        {
          if (factories.Length == 0)
            foreach (var x in dynamicRegistrations)
            {
              var d = x.Factory;
              if (d.FactoryType == FactoryType.Service && d.ValidateAndNormalizeRegistration(serviceType, serviceKey, false, Rules))
                factories = factories.Append(KV.Of(x.ServiceKey ?? (dynamicKey = dynamicKey?.Next() ?? DefaultDynamicKey.Value), d));
            }
          else
          {
            foreach (var x in dynamicRegistrations)
            {
              var d = x.Factory;
              if (d.FactoryType != FactoryType.Service || !d.ValidateAndNormalizeRegistration(serviceType, serviceKey, false, Rules))
                continue; // skip non-relevant factory types and invalid factories

              if (x.ServiceKey == null) // for the default dynamic factory
                switch (x.IfAlreadyRegistered)
                {
                  case IfAlreadyRegistered.Keep: // accept the default if result factories don't contain it already
                  case IfAlreadyRegistered.Throw:
                    if (factories.IndexOf(f => f.Key is DefaultKey || f.Key is DefaultDynamicKey) != -1)
                      continue; // skip if the factories are already containing the default factory
                    break;

                  case IfAlreadyRegistered.Replace: // remove the default from the result factories
                    factories = factories.Match(f => !(f.Key is DefaultKey || f.Key is DefaultDynamicKey));
                    break;

                  case IfAlreadyRegistered.AppendNotKeyed:
                    break;

                  case IfAlreadyRegistered.AppendNewImplementation:
                    if (d.CanAccessImplementationType &&
                        factories.IndexOf(d.ImplementationType, (it, f) => f.Value.CanAccessImplementationType && f.Value.ImplementationType == it) != -1)
                      continue; // skip if the factories contains the factory with the same dynamic implementation type
                    break;
                }
              else // for the keyed dynamic factory
                switch (x.IfAlreadyRegistered)
                {
                  case IfAlreadyRegistered.Replace:
                    factories = factories.Match(x.ServiceKey, (k, f) => !f.Key.Equals(k));
                    break; // remove from the factories the factory with the same key

                  default:
                    if (factories.IndexOf(x.ServiceKey, (k, f) => f.Key.Equals(k)) != -1)
                      continue; // keep the dynamic factory with the new service key, otherwise skip it
                    break;
                }

              factories = factories.Append(KV.Of(x.ServiceKey ?? (dynamicKey = dynamicKey?.Next() ?? DefaultDynamicKey.Value), d));
            }
          }
        }

        if (openGenericServiceType != null) // todo: @bug check if we need todo that for  AsFallback
        {
          dynamicRegistrations = dynamicRegistrationProvider(openGenericServiceType, serviceKey).ToArrayOrSelf();
          openGenericServiceType = null; // prevent the infinite loop
          goto restartWithOpenGenericRegistrations;
        }
      }
      return factories;
    }

    private Factory[] CombineRegisteredDecoratorWithDynamicFactories(Factory[] factories, Type serviceType)
    {
      var withFlags = DynamicRegistrationFlags.Decorator;
      if (serviceType == typeof(object))
        withFlags |= DynamicRegistrationFlags.DecoratorOfAnyTypeViaObjectServiceType;

      var withoutFlags = factories != null ? DynamicRegistrationFlags.AsFallback : DynamicRegistrationFlags.NoFlags;

      var dynamicFlags = Rules.DynamicRegistrationFlags;
      for (var i = 0; i < dynamicFlags.Length; ++i)
      {
        var flag = dynamicFlags[i];
        if ((flag & withFlags) != withFlags || (flag & withoutFlags) != 0)
          continue;

        var dynamicRegistrationProvider = Rules.DynamicRegistrationProviders[i];
        var dynamicRegistrations = dynamicRegistrationProvider(serviceType, null).ToArrayOrSelf();
        if (dynamicRegistrations.IsNullOrEmpty())
          continue;

        if (factories.IsNullOrEmpty())
        {
          foreach (var x in dynamicRegistrations)
          {
            var d = x.Factory;
            if (d.FactoryType == FactoryType.Decorator && d.ValidateAndNormalizeRegistration(serviceType, null, false, Rules))
              factories = factories.Append(d);
          }
          continue;
        }

        foreach (var x in dynamicRegistrations)
        {
          var d = x.Factory;
          if (d.FactoryType != FactoryType.Decorator || !d.ValidateAndNormalizeRegistration(serviceType, null, false, Rules))
            continue; // skip non-relevant factory types and invalid factories

          switch (x.IfAlreadyRegistered)
          {
            case IfAlreadyRegistered.Keep:
            case IfAlreadyRegistered.Throw:
              continue;

            case IfAlreadyRegistered.Replace:
              factories = Empty<Factory>(); // remove the default from the result factories
              break;

            case IfAlreadyRegistered.AppendNotKeyed:
              break;

            case IfAlreadyRegistered.AppendNewImplementation:
              if (d.CanAccessImplementationType &&
                  factories.IndexOf(d.ImplementationType, (it, f) => f.CanAccessImplementationType && f.ImplementationType == it) != -1)
                continue; // skip if the factories contains the factory with the same dynamic implementation type
              break;
          }
          factories = factories.Append(d);
        }
      }

      return factories;
    }

    private static readonly LastFactoryIDWinsComparer _lastFactoryIDWinsComparer = new LastFactoryIDWinsComparer();
    private struct LastFactoryIDWinsComparer : IComparer<KV<object, Factory>>
    {
      public int Compare(KV<object, Factory> first, KV<object, Factory> next) =>
          (first?.Value.FactoryID ?? 0) - (next?.Value.FactoryID ?? 0);
    }

    private Factory GetWrapperFactoryOrDefault(Request request)
    {
      // note: wrapper ignores the service key, and propagate the service key to wrapped service
      var serviceType = request.GetActualServiceType();

      var itemType = serviceType.GetArrayElementTypeOrNull();
      if (itemType != null)
        serviceType = typeof(IEnumerable<>).MakeGenericType(itemType);

      var factory = ((IContainer)this).GetWrapperFactoryOrDefault(serviceType);
      if (factory?.FactoryGenerator != null)
        factory = factory.FactoryGenerator.GetGeneratedFactory(request);

      if (factory == null)
        return null;

      var condition = factory.Setup.Condition;
      if (condition != null && !condition(request))
        return null;

      return factory;
    }

    #endregion

    #region Implementation

    private int _disposed;
    private StackTrace _disposeStackTrace;

    internal readonly Ref<Registry> _registry;

    private readonly IScope _singletonScope;
    private readonly IScope _ownCurrentScope;
    private readonly IScopeContext _scopeContext;
    private readonly IResolverContext _parent;

    internal sealed class InstanceFactory : Factory
    {
      public override Type ImplementationType { get; }
      public override bool HasRuntimeState => true;

      public InstanceFactory(object instance, Type instanceType, IReuse reuse, IScope scopeToAdd = null) : base(reuse)
      {
        ImplementationType = instanceType;
        scopeToAdd?.SetOrAdd(FactoryID, instance);
      }

      /// Switched off until I (or someone) will figure it out.
      public override bool UseInterpretation(Request request) => false;

      /// Tries to return instance directly from scope or singleton, and fallbacks to expression for decorator.
      public override FactoryDelegate GetDelegateOrDefault(Request request)
      {
        if (request.IsResolutionRoot)
        {
          var decoratedExpr = request.Container.GetDecoratorExpressionOrDefault(request.WithResolvedFactory(this));
          if (decoratedExpr != null)
            return decoratedExpr.CompileToFactoryDelegate(request.Rules.UseFastExpressionCompiler, request.Rules.UseInterpretation);
        }

        return GetInstanceFromScopeChainOrSingletons;
      }

      /// <summary>Called for Injection as dependency.</summary>
      public override Expression GetExpressionOrDefault(Request request)
      {
        request = request.WithResolvedFactory(this);
        return request.Container.GetDecoratorExpressionOrDefault(request)
            ?? CreateExpressionOrDefault(request);
      }

      public override Expression CreateExpressionOrDefault(Request request) =>
          Resolver.CreateResolutionExpression(request);

      #region Implementation

      private object GetInstanceFromScopeChainOrSingletons(IResolverContext r)
      {
        for (var scope = r.CurrentScope; scope != null; scope = scope.Parent)
        {
          var result = GetAndUnwrapOrDefault(scope, FactoryID);
          if (result != null)
            return result;
        }

        var instance = GetAndUnwrapOrDefault(r.SingletonScope, FactoryID);
        return instance.ThrowIfNull(Error.UnableToFindSingletonInstance);
      }

      private static object GetAndUnwrapOrDefault(IScope scope, int factoryId)
      {
        object value;
        if (!scope.TryGet(out value, factoryId))
          return null;
        return (value as WeakReference)?.Target.ThrowIfNull(Error.WeakRefReuseWrapperGCed)
           ?? (value as HiddenDisposable)?.Value
           ?? value;
      }

      #endregion
    }

    internal sealed class Registry
    {
      public static readonly Registry Empty = new Registry();
      public static readonly Registry Default = new Registry(WrappersSupport.Wrappers);

      // Factories:
      public readonly ImMap<ImMap.KValue<Type>> Services;
      // todo: we may use Factory or Factory[] as a value for decorators
      public readonly ImMap<ImMap.KValue<Type>> Decorators; // value is Factory[] 
      public readonly ImMap<ImMap.KValue<Type>> Wrappers;   // value is Factory

      internal const int CACHE_SLOT_COUNT = 16;
      internal const int CACHE_SLOT_COUNT_MASK = CACHE_SLOT_COUNT - 1;

      public sealed class Compiling
      {
        public readonly Expression Expression;
        public Compiling(Expression expression) => Expression = expression;
      }

      public ImMap<ImMap.KValue<Type>>[] DefaultFactoryCache;

      [MethodImpl((MethodImplOptions)256)]
      public ImMapEntry<ImMap.KValue<Type>> GetCachedDefaultFactoryOrDefault(int serviceTypeHash, Type serviceType)
      {
        // copy to local `cache` will prevent NRE if cache is set to null from outside
        var cache = DefaultFactoryCache;
        return cache == null ? null : cache[serviceTypeHash & CACHE_SLOT_COUNT_MASK]?.GetEntryOrDefault(serviceTypeHash, serviceType);
      }

      public void TryCacheDefaultFactory<T>(int serviceTypeHash, Type serviceType, T factory)
      {
        // Disable caching when no services registered, not to cache an empty collection wrapper or alike.
        if (Services.IsEmpty)
          return;

        if (DefaultFactoryCache == null)
          Interlocked.CompareExchange(ref DefaultFactoryCache, new ImMap<ImMap.KValue<Type>>[CACHE_SLOT_COUNT], null);

        ref var map = ref DefaultFactoryCache[serviceTypeHash & CACHE_SLOT_COUNT_MASK];
        if (map == null)
          Interlocked.CompareExchange(ref map, ImMap<ImMap.KValue<Type>>.Empty, null);

        var m = map;
        if (Interlocked.CompareExchange(ref map, m.AddOrUpdate(serviceTypeHash, serviceType, factory), m) != m)
          Ref.Swap(ref map, serviceTypeHash, serviceType, factory, (x, h, t, f) => x.AddOrUpdate(h, t, f));
      }

      internal sealed class KeyedFactoryCacheEntry
      {
        public readonly KeyedFactoryCacheEntry Rest;
        public readonly object Key;
        public object Factory;
        public KeyedFactoryCacheEntry(KeyedFactoryCacheEntry rest, object key, object factory)
        {
          Rest = rest;
          Key = key;
          Factory = factory;
        }
      }

      // Where key is `KV.Of(ServiceKey | ScopeName | RequiredServiceType | KV.Of(ServiceKey, ScopeName | RequiredServiceType) | ...)`
      // and value is `KeyedFactoryCacheEntries`
      public ImMap<ImMap.KValue<Type>>[] KeyedFactoryCache;

      [MethodImpl((MethodImplOptions)256)]
      public bool GetCachedKeyedFactoryOrDefault(int serviceTypeHash, Type serviceType, object key, out KeyedFactoryCacheEntry result)
      {
        result = null;
        var cache = KeyedFactoryCache;
        if (cache != null)
        {
          var entry = cache[serviceTypeHash & CACHE_SLOT_COUNT_MASK]?.GetEntryOrDefault(serviceTypeHash, serviceType);
          if (entry != null)
            for (var x = (KeyedFactoryCacheEntry)entry.Value.Value; x != null && result == null; x = x.Rest)
              if (ReferenceEquals(x.Key, key))
                result = x;
              else if (x.Key.Equals(key))
                result = x;
        }

        return result != null;
      }

      public void TryCacheKeyedFactory(int serviceTypeHash, Type serviceType, object key, object factory)
      {
        // Disable caching when no services registered, not to cache an empty collection wrapper or alike.
        if (Services.IsEmpty)
          return;

        if (KeyedFactoryCache == null)
          Interlocked.CompareExchange(ref KeyedFactoryCache, new ImMap<ImMap.KValue<Type>>[CACHE_SLOT_COUNT], null);

        ref var map = ref KeyedFactoryCache[serviceTypeHash & CACHE_SLOT_COUNT_MASK];
        if (map == null)
          Interlocked.CompareExchange(ref map, ImMap<ImMap.KValue<Type>>.Empty, null);

        var entry = map.GetEntryOrDefault(serviceTypeHash, serviceType);
        if (entry == null)
        {
          entry = new ImMapEntry<ImMap.KValue<Type>>(serviceTypeHash, new ImMap.KValue<Type>(serviceType, null));
          var oldMap = map;
          var newMap = oldMap.AddOrKeepEntry(entry);
          if (Interlocked.CompareExchange(ref map, newMap, oldMap) == oldMap)
          {
            if (newMap == oldMap)
              entry = map.GetEntryOrDefault(serviceTypeHash, serviceType);
          }
          else
            entry = Ref.SwapAndGetNewValue(ref map, entry, (x, en) => x.AddOrKeepEntry(en)).GetEntryOrDefault(serviceTypeHash, serviceType);
        }

        var e = entry.Value.Value;
        if (Interlocked.CompareExchange(ref entry.Value.Value, SetOrAddKeyedCacheFactory(e, key, factory), e) != e)
          Ref.Swap(ref entry.Value.Value, key, factory, SetOrAddKeyedCacheFactory);
      }

      private object SetOrAddKeyedCacheFactory(object x, object k, object f)
      {
        for (var entry = (KeyedFactoryCacheEntry)x; entry != null; entry = entry.Rest)
        {
          if (entry.Key.Equals(k))
          {
            entry.Factory = f;
            return x;
          }
        }

        return new KeyedFactoryCacheEntry((KeyedFactoryCacheEntry)x, k, f);
      }

      // The cases to store
      // | Singleton of (ConstantExpression e)
      // | Transient of (Expression e, int dependencyCount)
      // | Scoped    of (Expression e)
      // | ScopedTo  of (Expression e, object name)
      internal struct ExpressionCacheSlot
      {
        public Expression Expr;
        public object ScopeNameOrDependencyCount; // null - singleton | NoNameForScoped - scoped | TransientDependencyCount - transient | scope name
        public static readonly object NoNameForScoped = new object();
      }

      internal class TransientDependencyCount
      {
        public int Value;
        public TransientDependencyCount(int value) => Value = value;
      }

      ///<summary>The int key is the `FactoryID`</summary>
      public ImMap<ExpressionCacheSlot>[] FactoryExpressionCache;

      internal Expression GetCachedFactoryExpression(int factoryId,
          IReuse reuse, out ImMapEntry<Registry.ExpressionCacheSlot> entry)
      {
        entry = FactoryExpressionCache?[factoryId & CACHE_SLOT_COUNT_MASK]?.GetEntryOrDefault(factoryId);
        if (entry == null)
          return null;

        var expr = entry.Value.Expr;
        if (expr == null)
          return null; // could be null when the cache is reset by Unregister and IfAlreadyRegistered.Replace

        var scopeNameOrDependencyCount = entry.Value.ScopeNameOrDependencyCount;

        if (reuse is SingletonReuse)
          return scopeNameOrDependencyCount == null ? expr : null;

        if (reuse == Reuse.Transient)
          return scopeNameOrDependencyCount is TransientDependencyCount ? expr : null;

        if (reuse is CurrentScopeReuse scoped)
        {
          if (scoped.Name == null)
            return scopeNameOrDependencyCount == Registry.ExpressionCacheSlot.NoNameForScoped ? expr : null;

          if (ReferenceEquals(scoped.Name, scopeNameOrDependencyCount) || scoped.Name.Equals(scopeNameOrDependencyCount))
            return expr;
        }

        return null;
      }

      internal void CacheFactoryExpression(int factoryId,
          Expression expr, IReuse reuse, int dependencyCount, ImMapEntry<ExpressionCacheSlot> entry)
      {
        if (entry == null)
        {
          if (FactoryExpressionCache == null)
            Interlocked.CompareExchange(ref FactoryExpressionCache,
                new ImMap<ExpressionCacheSlot>[CACHE_SLOT_COUNT], null);

          ref var map = ref FactoryExpressionCache[factoryId & CACHE_SLOT_COUNT_MASK];
          if (map == null)
            Interlocked.CompareExchange(ref map, ImMap<ExpressionCacheSlot>.Empty, null);

          entry = map.GetEntryOrDefault(factoryId);
          if (entry == null)
          {
            entry = new ImMapEntry<ExpressionCacheSlot>(factoryId);
            var oldMap = map;
            var newMap = oldMap.AddOrKeepEntry(entry);
            if (Interlocked.CompareExchange(ref map, newMap, oldMap) == oldMap)
            {
              if (newMap == oldMap)
                entry = map.GetEntryOrDefault(factoryId);
            }
            else
              entry = Ref.SwapAndGetNewValue(ref map, entry, (x, e) => x.AddOrKeepEntry(e))
                  .GetEntryOrDefault(factoryId);
          }
        }

        entry.Value.Expr = expr;

        if (reuse == Reuse.Transient)
          entry.Value.ScopeNameOrDependencyCount = new TransientDependencyCount(dependencyCount);
        else if (reuse is CurrentScopeReuse scoped)
          entry.Value.ScopeNameOrDependencyCount = scoped.Name ?? ExpressionCacheSlot.NoNameForScoped;
      }

      internal readonly IsRegistryChangePermitted IsChangePermitted;

      private Registry(ImMap<ImMap.KValue<Type>> wrapperFactories = null)
          : this(ImMap<ImMap.KValue<Type>>.Empty, ImMap<ImMap.KValue<Type>>.Empty, wrapperFactories ?? ImMap<ImMap.KValue<Type>>.Empty,
              null, null, null, // caches are initialized to `null` to quickly check that they 
              IsRegistryChangePermitted.Permitted)
      { }

      private Registry(
          ImMap<ImMap.KValue<Type>> services,
          ImMap<ImMap.KValue<Type>> decorators,
          ImMap<ImMap.KValue<Type>> wrappers,
          ImMap<ImMap.KValue<Type>>[] defaultFactoryCache,
          ImMap<ImMap.KValue<Type>>[] keyedFactoryCache,
          ImMap<ExpressionCacheSlot>[] factoryExpressionCache,
          IsRegistryChangePermitted isChangePermitted)
      {
        Services = services;
        Decorators = decorators;
        Wrappers = wrappers;
        DefaultFactoryCache = defaultFactoryCache;
        KeyedFactoryCache = keyedFactoryCache;
        FactoryExpressionCache = factoryExpressionCache;
        IsChangePermitted = isChangePermitted;
      }

      public Registry WithoutCache() =>
          new Registry(Services, Decorators, Wrappers, null, null, null, IsChangePermitted);

      internal Registry WithServices(ImMap<ImMap.KValue<Type>> services) =>
          services == Services ? this :
          new Registry(services, Decorators, Wrappers,
              // Using Copy is fine when you have only the registrations because the caches will be null and no actual copy will be done.
              DefaultFactoryCache.Copy(), KeyedFactoryCache.Copy(), FactoryExpressionCache.Copy(),
              IsChangePermitted);

      private Registry WithDecorators(ImMap<ImMap.KValue<Type>> decorators) =>
          decorators == Decorators ? this :
          new Registry(Services, decorators, Wrappers,
              DefaultFactoryCache.Copy(), KeyedFactoryCache.Copy(), FactoryExpressionCache.Copy(), IsChangePermitted);

      private Registry WithWrappers(ImMap<ImMap.KValue<Type>> wrappers) =>
          wrappers == Wrappers ? this :
          new Registry(Services, Decorators, wrappers,
              DefaultFactoryCache.Copy(), KeyedFactoryCache.Copy(), FactoryExpressionCache.Copy(), IsChangePermitted);

      public IEnumerable<ServiceRegistrationInfo> GetServiceRegistrations()
      {
        foreach (var entry in Services.Enumerate())
        {
          var fe = entry.Value.Value;
          if (fe is Factory factory)
            yield return new ServiceRegistrationInfo(factory, entry.Value.Key, null);
          else if (fe != null) // maybe `null` for the unregistered service, see #412
          {
            var factories = ((FactoriesEntry)fe).Factories;
            foreach (var f in factories.Enumerate())
              yield return new ServiceRegistrationInfo(f.Value, entry.Value.Key, f.Key);
          }
        }
      }

      public Registry Register(Factory factory, Type serviceType, IfAlreadyRegistered ifAlreadyRegistered, object serviceKey)
      {
        if (IsChangePermitted != IsRegistryChangePermitted.Permitted)
          return IsChangePermitted == IsRegistryChangePermitted.Ignored ? this
              : Throw.For<Registry>(Error.NoMoreRegistrationsAllowed,
                  serviceType, serviceKey != null ? "with key " + serviceKey : string.Empty, factory);

        var serviceTypeHash = RuntimeHelpers.GetHashCode(serviceType);
        return factory.FactoryType == FactoryType.Service
                ? serviceKey == null
                    ? WithDefaultService(factory, serviceTypeHash, serviceType, ifAlreadyRegistered)
                    : WithKeyedService(factory, serviceTypeHash, serviceType, ifAlreadyRegistered, serviceKey)
            : factory.FactoryType == FactoryType.Decorator
                ? WithDecorators(Decorators.AddOrUpdate(serviceTypeHash, serviceType, factory.One(),
                    (_, of, nf) => of.To<Factory[]>().Append((Factory[])nf)))
                : WithWrappers(Wrappers.AddOrUpdate(serviceTypeHash, serviceType, factory));
      }

      public Factory[] GetRegisteredFactories(Type serviceType, object serviceKey, FactoryType factoryType)
      {
        serviceType = serviceType.ThrowIfNull();
        switch (factoryType)
        {
          case FactoryType.Wrapper:
            {
              // first checking for the explicitly provided say `MyWrapper<IMyService>`
              if (Wrappers.GetValueOrDefault(serviceType) is Factory wrapper)
                return wrapper.One();

              var openGenServiceType = serviceType.GetGenericDefinitionOrNull();
              if (openGenServiceType != null &&
                  Wrappers.GetValueOrDefault(openGenServiceType) is Factory openGenWrapper)
                return openGenWrapper.One();

              if (serviceType.GetArrayElementTypeOrNull() != null &&
                  Wrappers.GetValueOrDefault(typeof(IEnumerable<>)) is Factory collectionWrapper)
                return collectionWrapper.One();

              return null;
            }
          case FactoryType.Decorator:
            {
              var decorators = Decorators.GetValueOrDefault(serviceType) as Factory[];
              var openGenServiceType = serviceType.GetGenericDefinitionOrNull();
              if (openGenServiceType != null)
                decorators = decorators.Append(Decorators.GetValueOrDefault(openGenServiceType) as Factory[]);
              return decorators;
            }
          default:
            {
              var entry = Services.GetValueOrDefault(serviceType);
              if (entry == null)
                return null;

              if (entry is Factory factory)
                return serviceKey == null || DefaultKey.Value.Equals(serviceKey) ? factory.One() : null;

              var factories = ((FactoriesEntry)entry).Factories;
              if (serviceKey == null) // get all the factories
                return factories.Visit(new List<Factory>(), (x, l) => l.Add(x.Value)).ToArray();

              return factories.GetValueOrDefault(serviceKey)?.One();
            }
        }
      }

      public bool IsRegistered(Type serviceType, object serviceKey, FactoryType factoryType,
          Func<Factory, bool> condition)
      {
        serviceType = serviceType.ThrowIfNull();
        switch (factoryType)
        {
          case FactoryType.Wrapper:
            {
              // first checking for the explicitly provided say `MyWrapper<IMyService>`
              if (Wrappers.GetValueOrDefault(serviceType) is Factory wrapper &&
                  (condition == null || condition(wrapper)))
                return true;

              var openGenServiceType = serviceType.GetGenericDefinitionOrNull();
              if (openGenServiceType != null &&
                  Wrappers.GetValueOrDefault(openGenServiceType) is Factory openGenWrapper &&
                  (condition == null || condition(openGenWrapper)))
                return true;

              if (serviceType.GetArrayElementTypeOrNull() != null &&
                  Wrappers.GetValueOrDefault(typeof(IEnumerable<>)) is Factory collectionWrapper &&
                  (condition == null || condition(collectionWrapper)))
                return true;

              return false;
            }
          case FactoryType.Decorator:
            {
              if (Decorators.GetValueOrDefault(serviceType) is Factory[] decorators && decorators.Length != 0 &&
                  (condition == null || decorators.FindFirst(condition) != null))
                return true;

              var openGenServiceType = serviceType.GetGenericDefinitionOrNull();
              if (openGenServiceType != null &&
                  Decorators.GetValueOrDefault(openGenServiceType) is Factory[] openGenDecorators && openGenDecorators.Length != 0 &&
                  (condition == null || openGenDecorators.FindFirst(condition) != null))
                return true;

              return false;
            }
          default: // services
            {
              // note: We are not checking the open-generic for the closed-generic service type
              // to be able to explicitly understand what registration is available - open or the closed-generic
              var entry = Services.GetValueOrDefault(serviceType);
              if (entry == null)
                return false;

              if (entry is Factory factory)
                return serviceKey == null || DefaultKey.Value.Equals(serviceKey)
                    ? condition == null || condition(factory)
                    : false;

              var factories = ((FactoriesEntry)entry).Factories;
              if (serviceKey == null)
                return condition == null || factories.FindFirstOrDefault(f => condition(f.Value)) != null;

              factory = factories.GetValueOrDefault(serviceKey);
              return factory != null && (condition == null || condition(factory));
            }
        }
      }

      public bool ClearCache(int hash, Type serviceType, object serviceKey, FactoryType factoryType)
      {
        var factories = GetRegisteredFactories(serviceType, serviceKey, factoryType);
        if (factories.IsNullOrEmpty())
          return false;

        for (var i = 0; i < factories.Length; i++)
          DropFactoryCache(factories[i], hash, serviceType, serviceKey);

        return true;
      }

      private Registry WithDefaultService(Factory factory, int serviceTypeHash, Type serviceType, IfAlreadyRegistered ifAlreadyRegistered)
      {
        var services = Services;
        object newEntry = factory;
        var oldEntry = services.GetValueOrDefault(serviceTypeHash, serviceType);
        if (oldEntry != null)
        {
          switch (ifAlreadyRegistered)
          {
            case IfAlreadyRegistered.AppendNotKeyed:
              newEntry = oldEntry is FactoriesEntry fe
                  ? fe.With(factory)
                  : FactoriesEntry.Empty.WithTwo((Factory)oldEntry, factory);
              break;
            case IfAlreadyRegistered.Throw:
              newEntry = oldEntry is FactoriesEntry oldFactoriesEntry && oldFactoriesEntry.LastDefaultKey == null
                  ? oldFactoriesEntry.With(factory)
                  : Throw.For<object>(Error.UnableToRegisterDuplicateDefault, serviceType, factory, oldEntry);
              break;
            case IfAlreadyRegistered.Replace:
              if (oldEntry is FactoriesEntry facEntryToReplace)
              {
                if (facEntryToReplace.LastDefaultKey == null)
                  newEntry = facEntryToReplace.With(factory);
                else
                {
                  // remove defaults but keep keyed (issue #569) by collecting the only keyed factories
                  // and using them in a new factory entry
                  var keyedFactories = facEntryToReplace.Factories.Fold(
                      ImHashMap<object, Factory>.Empty,
                      (x, map) => x.Key is DefaultKey == false ? map.AddOrUpdate(x.Key, x.Value) : map);
                  if (!keyedFactories.IsEmpty)
                    newEntry = new FactoriesEntry(DefaultKey.Value,
                        keyedFactories.AddOrUpdate(DefaultKey.Value, factory));
                }
              }
              break;
            case IfAlreadyRegistered.AppendNewImplementation:
              var oldImplFacsEntry = oldEntry as FactoriesEntry;
              if (oldImplFacsEntry != null && oldImplFacsEntry.LastDefaultKey == null)
                newEntry = oldImplFacsEntry.With(factory);
              else
              {
                var oldFactory = oldEntry as Factory;
                var implementationType = factory.ImplementationType;
                if (implementationType == null ||
                    oldFactory != null && oldFactory.ImplementationType != implementationType)
                  newEntry = (oldImplFacsEntry ?? FactoriesEntry.Empty.With(oldFactory)).With(factory);
                else if (oldImplFacsEntry != null)
                {
                  var isNewImplType = true;
                  foreach (var f in oldImplFacsEntry.Factories.Enumerate())
                    if (f.Value.ImplementationType == implementationType)
                    {
                      isNewImplType = false;
                      break;
                    }

                  newEntry = isNewImplType
                      ? (oldImplFacsEntry ?? FactoriesEntry.Empty.With(oldFactory)).With(factory)
                      : oldEntry;
                }
              }
              break;
            default: // IfAlreadyRegisteredKeepDefaultService
              newEntry = oldEntry is FactoriesEntry oldFacsEntry && oldFacsEntry.LastDefaultKey == null
                  ? oldFacsEntry.With(factory)
                  : oldEntry;
              break;
          }
        }

        // services did not change
        if (newEntry == oldEntry)
          return this;

        var newServices = services.AddOrUpdate(serviceTypeHash, serviceType, newEntry);
        var newRegistry = new Registry(newServices, Decorators, Wrappers,
            DefaultFactoryCache.Copy(), KeyedFactoryCache.Copy(), FactoryExpressionCache.Copy(), IsChangePermitted);

        if (oldEntry != null)
        {
          if (oldEntry is Factory oldFactory)
            newRegistry.DropFactoryCache(oldFactory, serviceTypeHash, serviceType);
          else if (oldEntry is FactoriesEntry oldFactoriesEntry && oldFactoriesEntry?.LastDefaultKey != null)
            oldFactoriesEntry.Factories.Visit(new { newRegistry, serviceTypeHash, serviceType }, (x, s) =>
            {
              if (x.Key is DefaultKey)
                s.newRegistry.DropFactoryCache(x.Value, s.serviceTypeHash, s.serviceType);
            });
        }

        return newRegistry;
      }

      private Registry WithKeyedService(Factory factory, int serviceTypeHash, Type serviceType, IfAlreadyRegistered ifAlreadyRegistered, object serviceKey)
      {
        object newEntry = null;
        var services = Services;
        var oldEntry = services.GetValueOrDefault(serviceTypeHash, serviceType);
        if (oldEntry != null)
        {
          switch (ifAlreadyRegistered)
          {
            case IfAlreadyRegistered.Keep:
              if (oldEntry is Factory factoryToKeep)
                newEntry = FactoriesEntry.Empty.With(factory, serviceKey).With(factoryToKeep);
              else
              {
                var oldFacs = (FactoriesEntry)oldEntry;
                if (oldFacs.Factories.Contains(serviceKey))
                  return this; // keep the old registry
                newEntry = new FactoriesEntry(oldFacs.LastDefaultKey,
                    oldFacs.Factories.AddOrUpdate(serviceKey, factory));
              }
              break;
            case IfAlreadyRegistered.Replace:
              if (oldEntry is Factory factoryToReplace)
                newEntry = FactoriesEntry.Empty.With(factory, serviceKey).With(factoryToReplace);
              else
                newEntry = new FactoriesEntry(((FactoriesEntry)oldEntry).LastDefaultKey,
                    ((FactoriesEntry)oldEntry).Factories.AddOrUpdate(serviceKey, factory));
              break;
            default:
              if (oldEntry is Factory defaultFactory)
                newEntry = FactoriesEntry.Empty.With(factory, serviceKey).With(defaultFactory);
              else
              {
                var oldFacs = (FactoriesEntry)oldEntry;
                var oldFac = oldFacs.Factories.GetValueOrDefault(serviceKey);
                if (oldFac != null)
                  Throw.It(Error.UnableToRegisterDuplicateKey, serviceKey, serviceKey, oldFac);
                newEntry = new FactoriesEntry(oldFacs.LastDefaultKey,
                    ((FactoriesEntry)oldEntry).Factories.AddOrUpdate(serviceKey, factory));
              }
              break;
          }
        }

        if (newEntry == null)
          newEntry = FactoriesEntry.Empty.With(factory, serviceKey);

        var newServices = services.AddOrUpdate(serviceTypeHash, serviceType, newEntry);
        var newRegistry = new Registry(newServices, Decorators, Wrappers,
            DefaultFactoryCache.Copy(), KeyedFactoryCache.Copy(), FactoryExpressionCache.Copy(),
            IsChangePermitted);

        if (oldEntry != null && ifAlreadyRegistered == IfAlreadyRegistered.Replace &&
            oldEntry is FactoriesEntry updatedOldFactories &&
            updatedOldFactories.Factories.TryFind(serviceKey, out var droppedFactory))
          newRegistry.DropFactoryCache(droppedFactory, serviceTypeHash, serviceType, serviceKey);

        return newRegistry;
      }

      // todo: optimize allocations away
      public Registry Unregister(FactoryType factoryType, Type serviceType, object serviceKey, Func<Factory, bool> condition)
      {
        if (IsChangePermitted != IsRegistryChangePermitted.Permitted)
          return IsChangePermitted == IsRegistryChangePermitted.Ignored ? this
              : Throw.For<Registry>(Error.NoMoreUnregistrationsAllowed,
                  serviceType, serviceKey != null ? "with key " + serviceKey : string.Empty, factoryType);

        var serviceTypeHash = RuntimeHelpers.GetHashCode(serviceType);
        switch (factoryType)
        {
          case FactoryType.Wrapper:
            object removedWrapper = null;
            var registry = WithWrappers(Wrappers.Update(serviceTypeHash, serviceType, null, (_, factory, _null) =>
            {
              if (factory != null && condition != null && !condition((Factory)factory))
                return factory;
              removedWrapper = factory;
              return null;
            }));

            if (removedWrapper == null)
              return this;
            registry.DropFactoryCache((Factory)removedWrapper, serviceTypeHash, serviceType);
            return registry;

          case FactoryType.Decorator:
            Factory[] removedDecorators = null;
            // todo: minimize allocations in the lambdas below
            if (condition == null)
              registry = WithDecorators(Decorators.Update(serviceTypeHash, serviceType, null, (_, factories, _null) =>
              {
                removedDecorators = (Factory[])factories;
                return null;
              }));
            else
              registry = WithDecorators(Decorators.Update(serviceTypeHash, serviceType, null, (_, factories, _null) =>
              {
                removedDecorators = ((Factory[])factories).Match(condition);
                return removedDecorators == factories ? null : factories.To<Factory[]>().Except(removedDecorators).ToArray();
              }));

            if (removedDecorators.IsNullOrEmpty())
              return this;

            for (var i = 0; i < removedDecorators.Length; i++)
              registry.DropFactoryCache(removedDecorators[i], serviceTypeHash, serviceType);

            return registry;

          default:
            return UnregisterServiceFactory(serviceType, serviceKey, condition);
        }
      }

      // todo: optimize allocations away
      private Registry UnregisterServiceFactory(Type serviceType, object serviceKey = null, Func<Factory, bool> condition = null)
      {
        object removed = null; // Factory or FactoriesEntry or Factory[]
        ImMap<ImMap.KValue<Type>> services;
        var hash = RuntimeHelpers.GetHashCode(serviceType);
        if (serviceKey == null && condition == null) // simplest case with simplest handling
          services = Services.Update(hash, serviceType, null, (_, entry, _null) =>
          {
            removed = entry;
            return null;
          });
        else
          services = Services.Update(hash, serviceType, null, (_, entry, _null) =>
          {
            if (entry == null)
              return null;

            if (entry is Factory)
            {
              if ((serviceKey != null && !DefaultKey.Value.Equals(serviceKey)) ||
                  (condition != null && !condition((Factory)entry)))
                return entry; // keep entry
              removed = entry; // otherwise remove it (the only case if serviceKey == DefaultKey.Value)
              return null;
            }

            var factoriesEntry = (FactoriesEntry)entry;
            var oldFactories = factoriesEntry.Factories;
            var remainingFactories = ImHashMap<object, Factory>.Empty;
            if (serviceKey == null) // automatically means condition != null
            {
              // keep factories for which condition is true
              remainingFactories = oldFactories.Fold(remainingFactories,
                  (oldFac, remainingFacs) => condition != null && !condition(oldFac.Value)
                      ? remainingFacs.AddOrUpdate(oldFac.Key, oldFac.Value)
                      : remainingFacs);
            }
            else // serviceKey is not default, which automatically means condition == null
            {
              // set to null factory with specified key if its found
              remainingFactories = oldFactories;
              var factory = oldFactories.GetValueOrDefault(serviceKey);
              if (factory != null)
                remainingFactories = oldFactories.Height > 1
                    ? oldFactories.UpdateToDefault(serviceKey.GetHashCode(), serviceKey)
                    : ImHashMap<object, Factory>.Empty;
            }

            if (remainingFactories.IsEmpty)
            {
              // if no more remaining factories, then delete the whole entry
              removed = entry;
              return null;
            }

            // todo: huh - no perf here?
            removed = oldFactories.Enumerate().Except(remainingFactories.Enumerate()).Select(x => x.Value).ToArray();

            if (remainingFactories.Height == 1 && DefaultKey.Value.Equals(remainingFactories.Key))
              return remainingFactories.Value; // replace entry with single remaining default factory

            // update last default key if current default key was removed
            var newDefaultKey = factoriesEntry.LastDefaultKey;
            if (newDefaultKey != null && remainingFactories.GetValueOrDefault(newDefaultKey) == null)
              newDefaultKey = remainingFactories.Enumerate().Select(x => x.Key)
                  .OfType<DefaultKey>().OrderByDescending(key => key.RegistrationOrder).FirstOrDefault();
            return new FactoriesEntry(newDefaultKey, remainingFactories);
          });

        if (removed == null)
          return this;

        var registry = WithServices(services);

        if (removed is Factory f)
          registry.DropFactoryCache(f, hash, serviceType, serviceKey);
        else if (removed is Factory[] fs)
          foreach (var rf in fs)
            registry.DropFactoryCache(rf, hash, serviceType, serviceKey);
        else
          foreach (var e in ((FactoriesEntry)removed).Factories.Enumerate())
            registry.DropFactoryCache(e.Value, hash, serviceType, serviceKey);

        return registry;
      }

      internal void DropFactoryCache(Factory factory, int hash, Type serviceType, object serviceKey = null)
      {
        if (factory == null)
          return; // filter out Unregistered factory (see #390)

        if (DefaultFactoryCache != null || KeyedFactoryCache != null)
        {
          if (factory.FactoryGenerator == null)
          {
            var d = DefaultFactoryCache;
            if (d != null)
              Ref.Swap(ref d[hash & CACHE_SLOT_COUNT_MASK], hash, serviceType,
                  (x, h, t) => (x ?? ImMap<ImMap.KValue<Type>>.Empty).UpdateToDefault(h, t));

            var k = KeyedFactoryCache;
            if (k != null)
              Ref.Swap(ref k[hash & CACHE_SLOT_COUNT_MASK], hash, serviceType,
                  (x, h, t) => (x ?? ImMap<ImMap.KValue<Type>>.Empty).UpdateToDefault(h, t));
          }
          else
          {
            // We cannot remove generated factories, because they are keyed by implementation type and we may remove wrong factory
            // a safe alternative is dropping the whole cache
            DefaultFactoryCache = null;
            KeyedFactoryCache = null;
          }
        }

        if (FactoryExpressionCache != null)
        {
          var exprCache = FactoryExpressionCache;
          if (exprCache != null)
          {
            var factoryId = factory.FactoryID;
            Ref.Swap(ref exprCache[factoryId & CACHE_SLOT_COUNT_MASK],
                factoryId, (x, i) => (x ?? ImMap<ExpressionCacheSlot>.Empty).UpdateToDefault(i));
          }
        }
      }

      public Registry WithIsChangePermitted(IsRegistryChangePermitted isChangePermitted) =>
          new Registry(Services, Decorators, Wrappers, DefaultFactoryCache, KeyedFactoryCache, FactoryExpressionCache, isChangePermitted);
    }

    private Container(Rules rules, Ref<Registry> registry, IScope singletonScope,
        IScopeContext scopeContext = null, IScope ownCurrentScope = null,
        int disposed = 0, StackTrace disposeStackTrace = null,
        IResolverContext parent = null)
    {
      Rules = rules;

      _registry = registry;

      _singletonScope = singletonScope;
      _scopeContext = scopeContext;
      _ownCurrentScope = ownCurrentScope;

      _disposed = disposed;
      _disposeStackTrace = disposeStackTrace;

      _parent = parent;
    }

    private void SetInitialFactoryID()
    {
      var lastGeneratedId = 0;
      GetLastGeneratedFactoryID(ref lastGeneratedId);
      if (lastGeneratedId > Factory._lastFactoryID)
        Factory._lastFactoryID = lastGeneratedId + 1;
    }

    #endregion
  }

  /// Special service key with info about open-generic service type
  public sealed class OpenGenericTypeKey : IConvertibleToExpression
  {
    /// <summary>Open-generic required service-type</summary>
    public readonly Type RequiredServiceType;

    /// <summary>Optional key</summary>
    public readonly object ServiceKey;

    /// <summary>Constructs the thing</summary>
    public OpenGenericTypeKey(Type requiredServiceType, object serviceKey)
    {
      RequiredServiceType = requiredServiceType.ThrowIfNull();
      ServiceKey = serviceKey;
    }

    /// <inheritdoc />
    public override string ToString() =>
        new StringBuilder(nameof(OpenGenericTypeKey)).Append('(')
            .Print(RequiredServiceType).Append(", ").Print(ServiceKey)
            .Append(')').ToString();

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
      var other = obj as OpenGenericTypeKey;
      return other != null &&
             other.RequiredServiceType == RequiredServiceType &&
             Equals(other.ServiceKey, ServiceKey);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Hasher.Combine(RequiredServiceType, ServiceKey);

    /// <inheritdoc />
    public Expression ToExpression(Func<object, Expression> fallbackConverter) =>
        New(_ctor, Constant(RequiredServiceType, typeof(Type)), fallbackConverter(ServiceKey));

    private static readonly ConstructorInfo _ctor = typeof(OpenGenericTypeKey)
        .GetTypeInfo().DeclaredConstructors.First(x => x.GetParameters().Length == 2);
  }

  ///<summary>Hides/wraps object with disposable interface.</summary> 
  public sealed class HiddenDisposable
  {
    internal static ConstructorInfo Ctor = typeof(HiddenDisposable).GetTypeInfo().DeclaredConstructors.First();
    internal static FieldInfo ValueField = typeof(HiddenDisposable).GetTypeInfo().GetDeclaredField(nameof(Value));

    /// <summary>Wrapped value</summary>
    public readonly object Value;

    /// <summary>Wraps the value</summary>
    public HiddenDisposable(object value) { Value = value; }
  }

  /// Interpreter of expression - where possible uses knowledge of DryIoc internals to avoid reflection
  public static class Interpreter
  {
    /// Calls `TryInterpret` inside try-catch and unwraps/re-throws `ContainerException` from the reflection `TargetInvocationException`
    public static bool TryInterpretAndUnwrapContainerException(
        IResolverContext r, Expression expr, bool useFec, out object result)
    {
      try
      {
        return Interpreter.TryInterpret(r, expr, FactoryDelegateCompiler.ResolverContextParamExpr, r, null, useFec, out result);
      }
      catch (TargetInvocationException tex) when (tex.InnerException != null)
      {
        throw tex.InnerException.TryRethrowWithPreservedStackTrace();
      }
    }

    /// <summary>Stores parent lambda params and args</summary>
    public sealed class ParentLambdaArgs
    {
      /// <summary> Parent or the `null` for the root </summary>
      public readonly ParentLambdaArgs ParentWithArgs;

      /// <summary> Params </summary>
      public readonly object ParamExprs;

      /// <summary> Args </summary>
      public readonly object ParamValues;

      /// <summary>Constructs with parent parent or `null` for the root</summary>
      public ParentLambdaArgs(ParentLambdaArgs parentWithArgs, object paramExprs, object paramValues)
      {
        ParentWithArgs = parentWithArgs;
        ParamExprs = paramExprs;
        ParamValues = paramValues;
      }
    }

    /// <summary>Interprets passed expression</summary>
    public static bool TryInterpret(IResolverContext r, Expression expr,
        object paramExprs, object paramValues, ParentLambdaArgs parentArgs, bool useFec, out object result)
    {
      result = null;
      switch (expr.NodeType)
      {
        case ExprType.Constant:
          {
            result = ((ConstantExpression)expr).Value;
            return true;
          }
        case ExprType.New:
          {
            var newExpr = (NewExpression)expr;
            ConstantExpression a;
#if SUPPORTS_FAST_EXPRESSION_COMPILER
            var fewArgCount = newExpr.FewArgumentCount;
            if (fewArgCount >= 0)
            {
              if (fewArgCount == 0)
              {
                result = newExpr.Constructor.Invoke(ArrayTools.Empty<object>());
                return true;
              }

              object[] fewArgs;
              if (fewArgCount == 1)
              {
                fewArgs = new object[1];
                var singleArgExpr = ((OneArgumentNewExpression)newExpr).Argument;
                if ((a = singleArgExpr as ConstantExpression) != null)
                  fewArgs[0] = a.Value;
                else if (!TryInterpret(r, singleArgExpr, paramExprs, paramValues, parentArgs, useFec, out fewArgs[0]))
                  return false;
                result = newExpr.Constructor.Invoke(fewArgs);
                return true;
              }

              if (fewArgCount == 2)
              {
                var fewArgsExpr = (TwoArgumentsNewExpression)newExpr;
                fewArgs = new object[2];
                if ((a = fewArgsExpr.Argument0 as ConstantExpression) != null)
                  fewArgs[0] = a.Value;
                else if (!TryInterpret(r, fewArgsExpr.Argument0, paramExprs, paramValues, parentArgs, useFec, out fewArgs[0]))
                  return false;
                if ((a = fewArgsExpr.Argument1 as ConstantExpression) != null)
                  fewArgs[1] = a.Value;
                else if (!TryInterpret(r, fewArgsExpr.Argument1, paramExprs, paramValues, parentArgs, useFec, out fewArgs[1]))
                  return false;
                result = newExpr.Constructor.Invoke(fewArgs);
                return true;
              }

              if (fewArgCount == 3)
              {
                var fewArgsExpr = (ThreeArgumentsNewExpression)newExpr;
                fewArgs = new object[3];
                if ((a = fewArgsExpr.Argument0 as ConstantExpression) != null)
                  fewArgs[0] = a.Value;
                else if (!TryInterpret(r, fewArgsExpr.Argument0, paramExprs, paramValues, parentArgs, useFec, out fewArgs[0]))
                  return false;
                if ((a = fewArgsExpr.Argument1 as ConstantExpression) != null)
                  fewArgs[1] = a.Value;
                else if (!TryInterpret(r, fewArgsExpr.Argument1, paramExprs, paramValues, parentArgs, useFec, out fewArgs[1]))
                  return false;
                if ((a = fewArgsExpr.Argument2 as ConstantExpression) != null)
                  fewArgs[2] = a.Value;
                else if (!TryInterpret(r, fewArgsExpr.Argument2, paramExprs, paramValues, parentArgs, useFec, out fewArgs[2]))
                  return false;
                result = newExpr.Constructor.Invoke(fewArgs);
                return true;
              }

              if (fewArgCount == 4)
              {
                fewArgs = new object[4];
                var fewArgsExpr = (FourArgumentsNewExpression)newExpr;
                if (!TryInterpret(r, fewArgsExpr.Argument0, paramExprs, paramValues, parentArgs, useFec, out fewArgs[0]) ||
                    !TryInterpret(r, fewArgsExpr.Argument1, paramExprs, paramValues, parentArgs, useFec, out fewArgs[1]) ||
                    !TryInterpret(r, fewArgsExpr.Argument2, paramExprs, paramValues, parentArgs, useFec, out fewArgs[2]) ||
                    !TryInterpret(r, fewArgsExpr.Argument3, paramExprs, paramValues, parentArgs, useFec, out fewArgs[3]))
                  return false;
                result = newExpr.Constructor.Invoke(fewArgs);
                return true;
              }
              if (fewArgCount == 5)
              {
                fewArgs = new object[5];
                var fewArgsExpr = (FiveArgumentsNewExpression)newExpr;
                if (!TryInterpret(r, fewArgsExpr.Argument0, paramExprs, paramValues, parentArgs, useFec, out fewArgs[0]) ||
                    !TryInterpret(r, fewArgsExpr.Argument1, paramExprs, paramValues, parentArgs, useFec, out fewArgs[1]) ||
                    !TryInterpret(r, fewArgsExpr.Argument2, paramExprs, paramValues, parentArgs, useFec, out fewArgs[2]) ||
                    !TryInterpret(r, fewArgsExpr.Argument3, paramExprs, paramValues, parentArgs, useFec, out fewArgs[3]) ||
                    !TryInterpret(r, fewArgsExpr.Argument4, paramExprs, paramValues, parentArgs, useFec, out fewArgs[4]))
                  return false;
                result = newExpr.Constructor.Invoke(fewArgs);
                return true;
              }
            }
#endif
            var newArgs = newExpr.Arguments.ToListOrSelf();
            if (newArgs.Count == 0)
              result = newExpr.Constructor.Invoke(ArrayTools.Empty<object>());
            else
            {
              var args = new object[newArgs.Count];
              for (var i = 0; i < args.Length; i++)
              {
                if ((a = newArgs[i] as ConstantExpression) != null)
                  args[i] = a.Value;
                else if (!TryInterpret(r, newArgs[i], paramExprs, paramValues, parentArgs, useFec, out args[i]))
                  return false;
              }
              result = newExpr.Constructor.Invoke(args);
            }
            return true;
          }
        case ExprType.Call:
          {
            return TryInterpretMethodCall(r, expr, paramExprs, paramValues, parentArgs, useFec, ref result);
          }
        case ExprType.Convert:
          {
            object instance = null;
            var convertExpr = (UnaryExpression)expr;
            var operandExpr = convertExpr.Operand;
            if (operandExpr is MethodCallExpression m)
            {
              if (!TryInterpretMethodCall(r, m, paramExprs, paramValues, parentArgs, useFec, ref instance))
                return false;
            }
            else if (operandExpr is InvocationExpression invokeExpr &&
                 invokeExpr.Expression is ConstantExpression cd && cd.Value is FactoryDelegate facDel)
            {
              // The majority of cases the delegate will be a well known `FactoryDelegate` - so calling it directly
              var rArg = invokeExpr.Arguments[0]; // todo: @perf optimize for a OneArgumentInvocationExpression
              if (rArg == FactoryDelegateCompiler.ResolverContextParamExpr)
                result = facDel(r);
              else if (rArg == ResolverContext.RootOrSelfExpr)
                result = facDel(r.Root ?? r);
              else if (TryInterpret(r, rArg, paramExprs, paramValues, parentArgs, useFec, out var resolver))
                result = facDel((IResolverContext)resolver);
              else return false;
              return true;
            }
            else if (!TryInterpret(r, operandExpr, paramExprs, paramValues, parentArgs, useFec, out instance))
              return false;

            // skip conversion for null and for directly assignable type
            if (instance == null)
              result = instance;
            else if (convertExpr.Type.GetTypeInfo().IsAssignableFrom(instance.GetType().GetTypeInfo()))
              result = instance;
            else
              result = Converter.ConvertWithOperator(instance, convertExpr.Type, expr);
            return true;
          }
        case ExprType.MemberAccess:
          {
            var memberExpr = (MemberExpression)expr;
            var instanceExpr = memberExpr.Expression;
            object instance = null;
            if (instanceExpr != null && !TryInterpret(r, instanceExpr, paramExprs, paramValues, parentArgs, useFec, out instance))
              return false;

            if (memberExpr.Member is FieldInfo field)
            {
              result = field.GetValue(instance);
              return true;
            }

            if (memberExpr.Member is PropertyInfo prop)
            {
              result = prop.GetValue(instance, null);
              return true;
            }

            return false;
          }
        case ExprType.MemberInit:
          {
            var memberInit = (MemberInitExpression)expr;
            if (!TryInterpret(r, memberInit.NewExpression, paramExprs, paramValues, parentArgs, useFec, out var instance))
              return false;

            var bindings = memberInit.Bindings;
            for (var i = 0; i < bindings.Count; i++)
            {
              var binding = (MemberAssignment)bindings[i];
              if (!TryInterpret(r, binding.Expression, paramExprs, paramValues, parentArgs, useFec, out var memberValue))
                return false;

              var field = binding.Member as FieldInfo;
              if (field != null)
                field.SetValue(instance, memberValue);
              else
                ((PropertyInfo)binding.Member).SetValue(instance, memberValue, null);
            }

            result = instance;
            return true;
          }
        case ExprType.NewArrayInit:
          {
            var newArray = (NewArrayExpression)expr;
            var itemExprs = newArray.Expressions.ToListOrSelf();
            var items = new object[itemExprs.Count];

            for (var i = 0; i < items.Length; i++)
              if (!TryInterpret(r, itemExprs[i], paramExprs, paramValues, parentArgs, useFec, out items[i]))
                return false;

            result = Converter.ConvertMany(items, newArray.Type.GetElementType());
            return true;
          }
        case ExprType.Invoke:
          {
            var invokeExpr = (InvocationExpression)expr;
            var delegateExpr = invokeExpr.Expression;
            if (delegateExpr is ConstantExpression dc && dc.Value is FactoryDelegate facDel)
            {
              var rArg = invokeExpr.Arguments[0]; // todo: @perf optimize for a OneArgumentInvocationExpression
              if (rArg == FactoryDelegateCompiler.ResolverContextParamExpr)
                result = facDel(r);
              else if (rArg == ResolverContext.RootOrSelfExpr)
                result = facDel(r.Root ?? r);
              else if (TryInterpret(r, rArg, paramExprs, paramValues, parentArgs, useFec, out var resolver))
                result = facDel((IResolverContext)resolver);
              else return false;
              return true;
            }

            // The Invocation of Func is used for splitting the big object graphs
            // so we can ignore this split and go directly to the body
            if (delegateExpr.Type == typeof(Func<object>) && delegateExpr is LambdaExpression f)
              return TryInterpret(r, f.Body, paramExprs, paramValues, parentArgs, useFec, out result);

#if !SUPPORTS_DELEGATE_METHOD
                        return false;
#else
            if (!TryInterpret(r, delegateExpr, paramExprs, paramValues, parentArgs, useFec, out var delegateObj))
              return false;

            var lambda = (Delegate)delegateObj;
            var argExprs = invokeExpr.Arguments.ToListOrSelf(); // todo: @perf recognize the OneArgumentInvocationExpression
            if (argExprs.Count == 0)
              result = lambda.GetMethodInfo().Invoke(lambda.Target, ArrayTools.Empty<object>());
            else // it does not make sense to avoid array allocating for the single argument because we still need to pass array to the Invoke call
            {
              var args = new object[argExprs.Count];
              for (var i = 0; i < args.Length; i++)
                if (!TryInterpret(r, argExprs[i], paramExprs, paramValues, parentArgs, useFec, out args[i]))
                  return false;
              result = lambda.GetMethodInfo().Invoke(lambda.Target, args);
            }
            return true;
#endif
          }
        case ExprType.Parameter:
          {
            if (expr == paramExprs)
            {
              result = paramValues;
              return true;
            }

            if (paramExprs is IList<ParameterExpression> multipleParams)
              for (var i = 0; i < multipleParams.Count; i++)
                if (expr == multipleParams[i])
                {
                  result = ((object[])paramValues)[i];
                  return true;
                }

            if (parentArgs != null)
            {
              for (var p = parentArgs; p != null; p = p.ParentWithArgs)
              {
                if (expr == p.ParamExprs)
                {
                  result = p.ParamValues;
                  return true;
                }

                multipleParams = p.ParamExprs as IList<ParameterExpression>;
                if (multipleParams != null)
                  for (var i = 0; i < multipleParams.Count; i++)
                    if (expr == multipleParams[i])
                    {
                      result = ((object[])p.ParamValues)[i];
                      return true;
                    }
              }
            }
            return false;
          }
        case ExprType.Lambda:
          return TryInterpretNestedLambda(r, (LambdaExpression)expr, paramExprs, paramValues, parentArgs, useFec, ref result);
        default:
          return false;
      }
    }

    private static bool TryInterpretNestedLambda(IResolverContext r, LambdaExpression lambdaExpr,
        object paramExprs, object paramValues, ParentLambdaArgs parentArgs, bool useFec, ref object result)
    {
#if SUPPORTS_FAST_EXPRESSION_COMPILER
      var returnType = lambdaExpr.ReturnType;
#else
            var returnType = lambdaExpr.Type.GetTypeInfo().GetDeclaredMethod("Invoke").ReturnType;
#endif
      if (paramExprs != null)
        parentArgs = new ParentLambdaArgs(parentArgs, paramExprs, paramValues);

      var bodyExpr = lambdaExpr.Body;
      var lambdaParams = lambdaExpr.Parameters;
      var paramCount = lambdaParams.Count;
      if (paramCount == 0)
      {
        if (returnType != typeof(void))
        {
          result = new Func<object>(() => TryInterpretNestedLambdaBodyAndUnwrapException(r, bodyExpr, null, null, parentArgs, useFec));
          if (returnType != typeof(object))
            result = _convertFuncMethod.MakeGenericMethod(returnType).Invoke(null, new[] { result });
        }
        else
        {
          result = new Action(() => TryInterpretNestedLambdaBodyAndUnwrapException(r, bodyExpr, null, null, parentArgs, useFec));
        }
      }
      else if (paramCount == 1)
      {
        var paramExpr = lambdaParams[0];
        if (returnType != typeof(void))
        {
          result = new Func<object, object>(arg => TryInterpretNestedLambdaBodyAndUnwrapException(r, bodyExpr, paramExpr, arg, parentArgs, useFec));
          if (paramExpr.Type != typeof(object) || returnType != typeof(object))
            result = _convertOneArgFuncMethod.MakeGenericMethod(paramExpr.Type, returnType).Invoke(null, new[] { result });
        }
        else
        {
          result = new Action<object>(arg => TryInterpretNestedLambdaBodyAndUnwrapException(r, bodyExpr, paramExpr, arg, parentArgs, useFec));
          if (paramExpr.Type != typeof(object))
            result = _convertOneArgActionMethod.MakeGenericMethod(paramExpr.Type).Invoke(null, new[] { result });
        }
      }
      else if (paramCount == 2)
      {
        var paramExpr0 = lambdaParams[0];
        var paramExpr1 = lambdaParams[1];
        if (returnType != typeof(void))
        {
          result = new Func<object, object, object>((arg0, arg1) =>
              TryInterpretNestedLambdaBodyAndUnwrapException(r, bodyExpr, lambdaParams, new[] { arg0, arg1 }, parentArgs, useFec));

          if (paramExpr0.Type != typeof(object) || paramExpr1.Type != typeof(object) || returnType != typeof(object))
            result = _convertTwoArgFuncMethod.MakeGenericMethod(paramExpr0.Type, paramExpr1.Type, returnType).Invoke(null, new[] { result });
        }
        else
        {
          result = new Action<object, object>((arg0, arg1) =>
              TryInterpretNestedLambdaBodyAndUnwrapException(r, bodyExpr, lambdaParams, new[] { arg0, arg1 }, parentArgs, useFec));

          if (paramExpr0.Type != typeof(object) || paramExpr1.Type != typeof(object))
            result = _convertTwoArgActionMethod.MakeGenericMethod(paramExpr0.Type, paramExpr1.Type).Invoke(null, new[] { result });
        }
      }
      else if (paramCount == 3)
      {
        var paramExpr0 = lambdaParams[0];
        var paramExpr1 = lambdaParams[1];
        var paramExpr2 = lambdaParams[2];
        if (returnType != typeof(void))
        {
          result = new Func<object[], object>(args =>
              TryInterpretNestedLambdaBodyAndUnwrapException(r, bodyExpr, lambdaParams, args, parentArgs, useFec));
          result = _convertThreeArgFuncMethod.MakeGenericMethod(paramExpr0.Type, paramExpr1.Type, paramExpr2.Type, returnType)
              .Invoke(null, new[] { result });
        }
        else
        {
          result = new Action<object[]>(args =>
              TryInterpretNestedLambdaBodyAndUnwrapException(r, bodyExpr, lambdaParams, args, parentArgs, useFec));
          result = _convertThreeArgActionMethod.MakeGenericMethod(paramExpr0.Type, paramExpr1.Type, paramExpr2.Type)
              .Invoke(null, new[] { result });
        }
      }
      else if (paramCount == 4)
      {
        var paramExpr0 = lambdaParams[0];
        var paramExpr1 = lambdaParams[1];
        var paramExpr2 = lambdaParams[2];
        var paramExpr3 = lambdaParams[3];
        if (returnType != typeof(void))
        {
          result = new Func<object[], object>(args =>
              TryInterpretNestedLambdaBodyAndUnwrapException(r, bodyExpr, lambdaParams, args, parentArgs, useFec));
          result = _convertFourArgFuncMethod
              .MakeGenericMethod(paramExpr0.Type, paramExpr1.Type, paramExpr2.Type, paramExpr3.Type, returnType)
              .Invoke(null, new[] { result });
        }
        else
        {
          result = new Action<object[]>(args =>
              TryInterpretNestedLambdaBodyAndUnwrapException(r, bodyExpr, lambdaParams, args, parentArgs, useFec));
          result = _convertFourArgActionMethod
              .MakeGenericMethod(paramExpr0.Type, paramExpr1.Type, paramExpr2.Type, paramExpr3.Type)
              .Invoke(null, new[] { result });
        }
      }
      else
        return false;

      var resultType = result.GetType();
      var lambdaType = lambdaExpr.Type;
      if ((resultType.GetGenericDefinitionOrNull() ?? resultType) != (lambdaType.GetGenericDefinitionOrNull() ?? lambdaType))
      {
#if SUPPORTS_DELEGATE_METHOD
        result = ((Delegate)result).GetMethodInfo().CreateDelegate(lambdaType, ((Delegate)result).Target);
#else
                return false;
#endif
      }

      return true;
    }

    private static object TryInterpretNestedLambdaBodyAndUnwrapException(IResolverContext r,
        Expression bodyExpr, object paramExprs, object paramValues, ParentLambdaArgs parentArgs, bool useFec)
    {
      try
      {
        if (!TryInterpret(r, bodyExpr, paramExprs, paramValues, parentArgs, useFec, out var lambdaResult))
          Throw.It(Error.UnableToInterpretTheNestedLambda, bodyExpr);
        return lambdaResult;
      }
      catch (TargetInvocationException tex) when (tex.InnerException != null)
      {
        throw tex.InnerException.TryRethrowWithPreservedStackTrace();
      }
    }

    internal static Func<R> ConvertFunc<R>(Func<object> f) => () => (R)f();
    private static readonly MethodInfo _convertFuncMethod = typeof(Interpreter).GetTypeInfo().GetDeclaredMethod(nameof(ConvertFunc));

    internal static Func<T, R> ConvertOneArgFunc<T, R>(Func<object, object> f) => a => (R)f(a);
    private static readonly MethodInfo _convertOneArgFuncMethod = typeof(Interpreter).GetTypeInfo().GetDeclaredMethod(nameof(ConvertOneArgFunc));

    internal static Action<T> ConvertOneArgAction<T>(Action<object> f) => a => f(a);
    private static readonly MethodInfo _convertOneArgActionMethod = typeof(Interpreter).GetTypeInfo().GetDeclaredMethod(nameof(ConvertOneArgAction));

    internal static Func<T0, T1, R> ConvertTwoArgFunc<T0, T1, R>(Func<object, object, object> f) => (a0, a1) => (R)f(a0, a1);
    private static readonly MethodInfo _convertTwoArgFuncMethod = typeof(Interpreter).GetTypeInfo().GetDeclaredMethod(nameof(ConvertTwoArgFunc));

    internal static Action<T0, T1> ConvertTwoArgAction<T0, T1>(Action<object, object> f) => (a0, a1) => f(a0, a1);
    private static readonly MethodInfo _convertTwoArgActionMethod = typeof(Interpreter).GetTypeInfo().GetDeclaredMethod(nameof(ConvertTwoArgAction));

    internal static Func<T0, T1, T2, R> ConvertThreeArgFunc<T0, T1, T2, R>(Func<object[], object> f) => (a0, a1, a2) => (R)f(new object[] { a0, a1, a2 });
    private static readonly MethodInfo _convertThreeArgFuncMethod = typeof(Interpreter).GetTypeInfo().GetDeclaredMethod(nameof(ConvertThreeArgFunc));

    internal static Action<T0, T1, T2> ConvertThreeArgAction<T0, T1, T2>(Action<object[]> f) => (a0, a1, a2) => f(new object[] { a0, a1, a2 });
    private static readonly MethodInfo _convertThreeArgActionMethod = typeof(Interpreter).GetTypeInfo().GetDeclaredMethod(nameof(ConvertThreeArgAction));

    internal static Func<T0, T1, T2, T3, R> ConvertFourArgFunc<T0, T1, T2, T3, R>(Func<object[], object> f) => (a0, a1, a2, a3) => (R)f(new object[] { a0, a1, a2, a3 });
    private static readonly MethodInfo _convertFourArgFuncMethod = typeof(Interpreter).GetTypeInfo().GetDeclaredMethod(nameof(ConvertFourArgFunc));

    internal static Action<T0, T1, T2, T3> ConvertFourArgAction<T0, T1, T2, T3>(Action<object[]> f) => (a0, a1, a2, a3) => f(new object[] { a0, a1, a2, a3 });
    private static readonly MethodInfo _convertFourArgActionMethod = typeof(Interpreter).GetTypeInfo().GetDeclaredMethod(nameof(ConvertFourArgAction));

    private static bool TryInterpretMethodCall(IResolverContext r, Expression expr,
        object paramExprs, object paramValues, ParentLambdaArgs parentArgs, bool useFec, ref object result)
    {
      if (ReferenceEquals(expr, ResolverContext.RootOrSelfExpr))
      {
        result = r.Root ?? r;
        return true;
      }

      var callExpr = (MethodCallExpression)expr;
      var method = callExpr.Method;
      var methodDeclaringType = method.DeclaringType;

      if (methodDeclaringType == typeof(CurrentScopeReuse))
      {
        if (method == CurrentScopeReuse.GetScopedViaFactoryDelegateNoDisposalIndexMethod)
        {
          result = InterpretGetScopedViaFactoryDelegateNoDisposalIndex(r, callExpr, paramExprs, paramValues, parentArgs, useFec);
          return true;
        }

        if (method == CurrentScopeReuse.GetScopedViaFactoryDelegateMethod)
        {
          result = InterpretGetScopedViaFactoryDelegate(r, callExpr, paramExprs, paramValues, parentArgs, useFec);
          return true;
        }

        if (method == CurrentScopeReuse.GetNameScopedViaFactoryDelegateMethod)
        {
          result = InterpretGetNameScopedViaFactoryDelegate(r, callExpr, paramExprs, paramValues, parentArgs, useFec);
          return true;
        }

        if (method == CurrentScopeReuse.GetScopedOrSingletonViaFactoryDelegateMethod)
        {
          result = InterpretGetScopedOrSingletonViaFactoryDelegate(r, callExpr, paramExprs, paramValues, parentArgs, useFec);
          return true;
        }

        var callArgs = callExpr.Arguments.ToListOrSelf(); // todo: Check for the few arguments method call expression
        var resolver = r;
        if (!ReferenceEquals(callArgs[0], FactoryDelegateCompiler.ResolverContextParamExpr))
        {
          if (!TryInterpret(resolver, callArgs[0], paramExprs, paramValues, parentArgs, useFec, out var resolverObj))
            return false;
          resolver = (IResolverContext)resolverObj;
        }

        if (method == CurrentScopeReuse.TrackScopedOrSingletonMethod)
        {
          if (!TryInterpret(resolver, callArgs[1], paramExprs, paramValues, parentArgs, useFec, out var service))
            return false;
          result = CurrentScopeReuse.TrackScopedOrSingleton(resolver, service);
          return true;
        }

        if (method == CurrentScopeReuse.TrackScopedMethod)
        {
          var scope = resolver.GetCurrentScope((bool)((ConstantExpression)callArgs[1]).Value);
          if (scope == null)
            result = null; // result is null in this case
          else
          {
            if (!TryInterpret(resolver, callArgs[2], paramExprs, paramValues, parentArgs, useFec, out var service))
              return false;
            result = service is IDisposable d ? scope.TrackDisposableWithoutDisposalOrder(d) : service;
          }

          return true;
        }

        if (method == CurrentScopeReuse.TrackNameScopedMethod)
        {
          var scope = resolver.GetNamedScope(ConstValue(callArgs[1]), (bool)ConstValue(callArgs[2]));
          if (scope == null)
            result = null; // result is null in this case
          else
          {
            if (!TryInterpret(resolver, callArgs[3], paramExprs, paramValues, parentArgs, useFec, out var service))
              return false;
            result = service is IDisposable d ? scope.TrackDisposableWithoutDisposalOrder(d) : service;
          }

          return true;
        }
      }
      else if (methodDeclaringType == typeof(IScope))
      {
        var callArgs = callExpr.Arguments.ToListOrSelf();
        if (method == Scope.GetOrAddViaFactoryDelegateMethod)
        {
          r = r.Root ?? r;

          // check if scoped dependency is already in scope, then just return it
          var factoryId = (int)ConstValue(callArgs[0]);
          if (!r.SingletonScope.TryGet(out result, factoryId))
          {
            result = r.SingletonScope.TryGetOrAddWithoutClosure(factoryId, r,
                ((LambdaExpression)callArgs[1]).Body, useFec,
                (rc, e, uf) =>
                {
                  if (TryInterpret(rc, e, paramExprs, paramValues, parentArgs, uf, out var value))
                    return value;
                  return e.CompileToFactoryDelegate(uf, ((IContainer)rc).Rules.UseInterpretation)(rc);
                },
                (int)ConstValue(callArgs[3]));
          }

          return true;
        }

        if (method == Scope.TrackDisposableMethod)
        {
          r = r.Root ?? r;
          if (!TryInterpret(r, callArgs[0], paramExprs, paramValues, parentArgs, useFec, out var service))
            return false;
          if (service is IDisposable d)
          {
            var disposalOrder = (int)ConstValue(callArgs[1]);
            result = disposalOrder == 0
                ? r.SingletonScope.TrackDisposableWithoutDisposalOrder(d)
                : r.SingletonScope.TrackDisposable(service, disposalOrder);
          }

          return true;
        }
      }
      else if (methodDeclaringType == typeof(IResolver))
      {
        var resolver = r;
        if (!ReferenceEquals(callExpr.Object, FactoryDelegateCompiler.ResolverContextParamExpr))
        {
          if (!TryInterpret(resolver, callExpr.Object, paramExprs, paramValues, parentArgs, useFec, out var resolverObj))
            return false;
          resolver = (IResolverContext)resolverObj;
        }

        var callArgs = callExpr.Arguments.ToListOrSelf();
        if (method == Resolver.ResolveFastMethod)
        {
          result = resolver.Resolve((Type)ConstValue(callArgs[0]), (IfUnresolved)ConstValue(callArgs[1]));
          return true;
        }

        if (method == Resolver.ResolveMethod)
        {
          InterpretResolveMethod(resolver, callArgs, paramExprs, paramValues, parentArgs, useFec, out result);
          return true;
        }

        if (method == Resolver.ResolveManyMethod)
        {
          object serviceKey = null, preResolveParent = null, resolveArgs = null;
          if (!TryInterpret(resolver, callArgs[1], paramExprs, paramValues, parentArgs, useFec, out serviceKey) ||
              !TryInterpret(resolver, callArgs[3], paramExprs, paramValues, parentArgs, useFec, out preResolveParent) ||
              !TryInterpret(resolver, callArgs[4], paramExprs, paramValues, parentArgs, useFec, out resolveArgs))
            return false;

          result = resolver.ResolveMany((Type)ConstValue(callArgs[0]), serviceKey, (Type)ConstValue(callArgs[2]),
              (Request)preResolveParent, (object[])resolveArgs);
          return true;
        }
      }

      // fallback to reflection invocation
      object instance = null;
      var callObjectExpr = callExpr.Object;
      if (callObjectExpr != null)
      {
        if (callObjectExpr is ConstantExpression objConst)
          instance = objConst.Value;
        else if (!TryInterpret(r, callObjectExpr, paramExprs, paramValues, parentArgs, useFec, out instance))
          return false;
      }

#if SUPPORTS_FAST_EXPRESSION_COMPILER
      var fewArgCount = callExpr.FewArgumentCount;
      if (fewArgCount >= 0)
      {
        if (fewArgCount == 0)
        {
          result = callExpr.Method.Invoke(instance, ArrayTools.Empty<object>());
          return true;
        }

        if (fewArgCount == 1)
        {
          var fewArgs = new object[1];
          var fewArgsExpr = ((OneArgumentMethodCallExpression)callExpr).Argument;
          if (!TryInterpret(r, fewArgsExpr, paramExprs, paramValues, parentArgs, useFec, out fewArgs[0]))
            return false;
          result = callExpr.Method.Invoke(instance, fewArgs);
          return true;
        }

        if (fewArgCount == 2)
        {
          var fewArgs = new object[2];
          var fewArgsExpr = ((TwoArgumentsMethodCallExpression)callExpr);
          if (!TryInterpret(r, fewArgsExpr.Argument0, paramExprs, paramValues, parentArgs, useFec, out fewArgs[0]) ||
              !TryInterpret(r, fewArgsExpr.Argument1, paramExprs, paramValues, parentArgs, useFec, out fewArgs[1]))
            return false;
          result = callExpr.Method.Invoke(instance, fewArgs);
          return true;
        }

        if (fewArgCount == 3)
        {
          var fewArgs = new object[3];
          var fewArgsExpr = ((ThreeArgumentsMethodCallExpression)callExpr);
          if (!TryInterpret(r, fewArgsExpr.Argument0, paramExprs, paramValues, parentArgs, useFec, out fewArgs[0]) ||
              !TryInterpret(r, fewArgsExpr.Argument1, paramExprs, paramValues, parentArgs, useFec, out fewArgs[1]) ||
              !TryInterpret(r, fewArgsExpr.Argument2, paramExprs, paramValues, parentArgs, useFec, out fewArgs[2]))
            return false;
          result = callExpr.Method.Invoke(instance, fewArgs);
          return true;
        }

        if (fewArgCount == 4)
        {
          var fewArgs = new object[4];
          var fewArgsExpr = ((FourArgumentsMethodCallExpression)callExpr);
          if (!TryInterpret(r, fewArgsExpr.Argument0, paramExprs, paramValues, parentArgs, useFec, out fewArgs[0]) ||
              !TryInterpret(r, fewArgsExpr.Argument1, paramExprs, paramValues, parentArgs, useFec, out fewArgs[1]) ||
              !TryInterpret(r, fewArgsExpr.Argument2, paramExprs, paramValues, parentArgs, useFec, out fewArgs[2]) ||
              !TryInterpret(r, fewArgsExpr.Argument3, paramExprs, paramValues, parentArgs, useFec, out fewArgs[3]))
            return false;
          result = callExpr.Method.Invoke(instance, fewArgs);
          return true;
        }
        if (fewArgCount == 5)
        {
          var fewArgs = new object[5];
          var fewArgsExpr = ((FiveArgumentsMethodCallExpression)callExpr);
          if (!TryInterpret(r, fewArgsExpr.Argument0, paramExprs, paramValues, parentArgs, useFec, out fewArgs[0]) ||
              !TryInterpret(r, fewArgsExpr.Argument1, paramExprs, paramValues, parentArgs, useFec, out fewArgs[1]) ||
              !TryInterpret(r, fewArgsExpr.Argument2, paramExprs, paramValues, parentArgs, useFec, out fewArgs[2]) ||
              !TryInterpret(r, fewArgsExpr.Argument3, paramExprs, paramValues, parentArgs, useFec, out fewArgs[3]) ||
              !TryInterpret(r, fewArgsExpr.Argument4, paramExprs, paramValues, parentArgs, useFec, out fewArgs[4]))
            return false;
          result = callExpr.Method.Invoke(instance, fewArgs);
          return true;
        }
      }
#endif
      var args = callExpr.Arguments.ToListOrSelf();
      var callArgCount = args.Count;
      if (callArgCount == 0)
        result = method.Invoke(instance, ArrayTools.Empty<object>());
      else
      {
        var argObjects = new object[callArgCount];
        for (var i = 0; i < argObjects.Length; i++)
          if (!TryInterpret(r, args[i], paramExprs, paramValues, parentArgs, useFec, out argObjects[i]))
            return false;
        result = method.Invoke(instance, argObjects);
      }

      return true;
    }

    internal static void InterpretResolveMethod(IResolverContext resolver, IList<Expression> callArgs,
        object paramExprs, object paramValues, ParentLambdaArgs parentArgs, bool useFec, out object result)
    {
      TryInterpret(resolver, callArgs[1], paramExprs, paramValues, parentArgs, useFec, out var serviceKey);
      TryInterpret(resolver, callArgs[4], paramExprs, paramValues, parentArgs, useFec, out var preResolveParent);
      TryInterpret(resolver, callArgs[5], paramExprs, paramValues, parentArgs, useFec, out var resolveArgs);

      result = resolver.Resolve((Type)
          ((ConstantExpression)callArgs[0]).Value,
          serviceKey,
          (IfUnresolved)((ConstantExpression)callArgs[2]).Value,
          (Type)((ConstantExpression)callArgs[3]).Value,
          (Request)preResolveParent,
          (object[])resolveArgs);
    }

    private static object InterpretGetScopedViaFactoryDelegateNoDisposalIndex(IResolverContext r,
        MethodCallExpression callExpr, object paramExprs, object paramValues, ParentLambdaArgs parentArgs, bool useFec)
    {
#if SUPPORTS_FAST_EXPRESSION_COMPILER
      var fewArgExpr = (FourArgumentsMethodCallExpression)callExpr;
      var resolverArg = fewArgExpr.Argument0;
#else
            var args = callExpr.Arguments.ToListOrSelf();
            var resolverArg = args[0];
#endif
      if (!ReferenceEquals(resolverArg, FactoryDelegateCompiler.ResolverContextParamExpr))
      {
        if (!TryInterpret(r, resolverArg, paramExprs, paramValues, parentArgs, useFec, out var resolverObj))
          return false;
        r = (IResolverContext)resolverObj;
      }

      var scope = (Scope)r.CurrentScope;
      if (scope == null)
      {
#if SUPPORTS_FAST_EXPRESSION_COMPILER
        var throwIfNoScopeArg = fewArgExpr.Argument1;
#else
                var throwIfNoScopeArg = args[1];
#endif
        return (bool)((ConstantExpression)throwIfNoScopeArg).Value ? Throw.For<IScope>(Error.NoCurrentScope, r) : null;
      }

#if SUPPORTS_FAST_EXPRESSION_COMPILER
      var factoryIdArg = fewArgExpr.Argument2;
#else
            var factoryIdArg = args[2];
#endif
      var id = (int)((ConstantExpression)factoryIdArg).Value;
      ref var map = ref scope._maps[id & Scope.MAP_COUNT_SUFFIX_MASK];
      var itemRef = map.GetEntryOrDefault(id);
      if (itemRef != null && itemRef.Value != Scope.NoItem)
        return itemRef.Value;

      if (scope.IsDisposed)
        Throw.It(Error.ScopeIsDisposed, scope.ToString());

      itemRef = new ImMapEntry<object>(id, Scope.NoItem);
      var oldMap = map;
      var newMap = oldMap.AddOrKeepEntry(itemRef);
      if (Interlocked.CompareExchange(ref map, newMap, oldMap) != oldMap)
      {
        newMap = Ref.SwapAndGetNewValue(ref map, itemRef, (x, i) => x.AddOrKeepEntry(i));
        var otherItemRef = newMap.GetSurePresentEntry(id);
        if (otherItemRef != itemRef)
          return otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
      }
      else if (newMap == oldMap)
      {
        var otherItemRef = newMap.GetSurePresentEntry(id);
        return otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
      }

#if SUPPORTS_FAST_EXPRESSION_COMPILER
      var lambda = fewArgExpr.Argument3;
#else
            var lambda = args[3];
#endif

      object result = null;
#if SUPPORTS_SPIN_WAIT
      if (lambda is ConstantExpression lambdaConstExpr)
        result = ((FactoryDelegate)lambdaConstExpr.Value)(r);
      else if (!TryInterpret(r, ((LambdaExpression)lambda).Body, paramExprs, paramValues, parentArgs, useFec, out result))
        result = ((LambdaExpression)lambda).Body.CompileToFactoryDelegate(useFec, ((IContainer)r).Rules.UseInterpretation)(r);
      itemRef.Value = result;
#else
            lock (itemRef) 
            {
                if (lambda is ConstantExpression lambdaConstExpr)
                    result = ((FactoryDelegate)lambdaConstExpr.Value)(r);
                else if (!TryInterpret(r, ((LambdaExpression)lambda).Body, paramExprs, paramValues, parentArgs, useFec, out result))
                    result = ((LambdaExpression)lambda).Body.CompileToFactoryDelegate(useFec, ((IContainer)r).Rules.UseInterpretation)(r);
                
                itemRef.Value = result;
                Monitor.PulseAll(itemRef);
            }
#endif
      if (result is IDisposable disp && !ReferenceEquals(disp, scope))
        scope.AddUnorderedDisposable(disp);
      return result;
    }

    private static object InterpretGetScopedViaFactoryDelegate(IResolverContext r,
        MethodCallExpression callExpr, object paramExprs, object paramValues, ParentLambdaArgs parentArgs, bool useFec)
    {
#if SUPPORTS_FAST_EXPRESSION_COMPILER
      var fewArgExpr = (FiveArgumentsMethodCallExpression)callExpr;
      var resolverArg = fewArgExpr.Argument0;
#else
            var args = callExpr.Arguments.ToListOrSelf();
            var resolverArg = args[0];
#endif
      if (!ReferenceEquals(resolverArg, FactoryDelegateCompiler.ResolverContextParamExpr))
      {
        if (!TryInterpret(r, resolverArg, paramExprs, paramValues, parentArgs, useFec, out var resolverObj))
          return false;
        r = (IResolverContext)resolverObj;
      }

      var scope = (Scope)r.CurrentScope;
      if (scope == null)
      {
#if SUPPORTS_FAST_EXPRESSION_COMPILER
        var throwIfNoScopeArg = fewArgExpr.Argument1;
#else
                var throwIfNoScopeArg = args[1];
#endif
        return (bool)((ConstantExpression)throwIfNoScopeArg).Value ? Throw.For<IScope>(Error.NoCurrentScope, r) : null;
      }

#if SUPPORTS_FAST_EXPRESSION_COMPILER
      var factoryIdArg = fewArgExpr.Argument2;
#else
            var factoryIdArg = args[2];
#endif
      var id = (int)((ConstantExpression)factoryIdArg).Value;
      ref var map = ref scope._maps[id & Scope.MAP_COUNT_SUFFIX_MASK];
      var itemRef = map.GetEntryOrDefault(id);
      if (itemRef != null && itemRef.Value != Scope.NoItem)
        return itemRef.Value;

      if (scope.IsDisposed)
        Throw.It(Error.ScopeIsDisposed, scope.ToString());

      itemRef = new ImMapEntry<object>(id, Scope.NoItem);
      var oldMap = map;
      var newMap = oldMap.AddOrKeepEntry(itemRef);
      if (Interlocked.CompareExchange(ref map, newMap, oldMap) != oldMap)
      {
        newMap = Ref.SwapAndGetNewValue(ref map, itemRef, (x, i) => x.AddOrKeepEntry(i));
        var otherItemRef = newMap.GetSurePresentEntry(id);
        if (otherItemRef != itemRef)
          return otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
      }
      else if (newMap == oldMap)
      {
        var otherItemRef = newMap.GetSurePresentEntry(id);
        return otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
      }

#if SUPPORTS_FAST_EXPRESSION_COMPILER
      var lambda = fewArgExpr.Argument3;
#else
            var lambda = args[3];
#endif

      object result = null;
#if SUPPORTS_SPIN_WAIT
      if (lambda is ConstantExpression lambdaConstExpr)
        result = ((FactoryDelegate)lambdaConstExpr.Value)(r);
      else if (!TryInterpret(r, ((LambdaExpression)lambda).Body, paramExprs, paramValues, parentArgs, useFec, out result))
        result = ((LambdaExpression)lambda).Body.CompileToFactoryDelegate(useFec, ((IContainer)r).Rules.UseInterpretation)(r);
      itemRef.Value = result;
#else
            lock (itemRef) 
            {
                if (lambda is ConstantExpression lambdaConstExpr)
                    result = ((FactoryDelegate)lambdaConstExpr.Value)(r);
                else if (!TryInterpret(r, ((LambdaExpression)lambda).Body, paramExprs, paramValues, parentArgs, useFec, out result))
                    result = ((LambdaExpression)lambda).Body.CompileToFactoryDelegate(useFec, ((IContainer)r).Rules.UseInterpretation)(r);
                
                itemRef.Value = result;
                Monitor.PulseAll(itemRef);
            }
#endif
      if (result is IDisposable disp && !ReferenceEquals(disp, scope))
      {
#if SUPPORTS_FAST_EXPRESSION_COMPILER
        var disposalOrderArg = fewArgExpr.Argument4;
#else
                var disposalOrderArg = args[4];
#endif
        var disposalOrder = (int)((ConstantExpression)disposalOrderArg).Value;
        if (disposalOrder == 0)
          scope.AddUnorderedDisposable(disp);
        else
          scope.AddDisposable(disp, disposalOrder);
      }

      return result;
    }

    // todo: @perf create the overload without disposal index so we could use FiveArgumentsMethodCall expression from the FEC
    private static object InterpretGetNameScopedViaFactoryDelegate(IResolverContext r,
        MethodCallExpression callExpr, object paramExprs, object paramValues, ParentLambdaArgs parentArgs, bool useFec)
    {
      var args = callExpr.Arguments.ToListOrSelf();
      if (!ReferenceEquals(args[0], FactoryDelegateCompiler.ResolverContextParamExpr))
      {
        if (!TryInterpret(r, args[0], paramExprs, paramValues, parentArgs, useFec, out var resolverObj))
          return false;
        r = (IResolverContext)resolverObj;
      }

      var scope = (Scope)r.GetNamedScope(((ConstantExpression)args[1]).Value, (bool)((ConstantExpression)args[2]).Value);
      if (scope == null)
        return null; // result is null in this case

      if (scope.IsDisposed)
        Throw.It(Error.ScopeIsDisposed, scope.ToString());

      // check if scoped dependency is already in scope, then just return it
      var id = (int)((ConstantExpression)args[3]).Value;
      ref var map = ref scope._maps[id & Scope.MAP_COUNT_SUFFIX_MASK];
      var itemRef = map.GetEntryOrDefault(id);
      if (itemRef != null && itemRef.Value != Scope.NoItem)
        return itemRef.Value;

      itemRef = new ImMapEntry<object>(id, Scope.NoItem);
      var oldMap = map;
      var newMap = oldMap.AddOrKeepEntry(itemRef);
      if (Interlocked.CompareExchange(ref map, newMap, oldMap) != oldMap)
      {
        newMap = Ref.SwapAndGetNewValue(ref map, itemRef, (x, i) => x.AddOrKeepEntry(i));
        var otherItemRef = newMap.GetSurePresentEntry(id);
        if (otherItemRef != itemRef)
          return otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
      }
      else if (newMap == oldMap)
      {
        var otherItemRef = newMap.GetSurePresentEntry(id);
        return otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
      }

      var lambda = args[4];
      object result = null;
#if SUPPORTS_SPIN_WAIT
      if (lambda is ConstantExpression lambdaConstExpr)
        result = ((FactoryDelegate)lambdaConstExpr.Value)(r);
      else if (!TryInterpret(r, ((LambdaExpression)lambda).Body, paramExprs, paramValues, parentArgs, useFec, out result))
        result = ((LambdaExpression)lambda).Body.CompileToFactoryDelegate(useFec, ((IContainer)r).Rules.UseInterpretation)(r);
      itemRef.Value = result;
#else
            lock (itemRef)
            {
                if (lambda is ConstantExpression lambdaConstExpr)
                    result = ((FactoryDelegate)lambdaConstExpr.Value)(r);
                else if (!TryInterpret(r, ((LambdaExpression)lambda).Body, paramExprs, paramValues, parentArgs, useFec, out result))
                    result = ((LambdaExpression)lambda).Body.CompileToFactoryDelegate(useFec, ((IContainer)r).Rules.UseInterpretation)(r);
                
                itemRef.Value = result;
                Monitor.PulseAll(itemRef);
            }
#endif
      if (result is IDisposable disp && !ReferenceEquals(disp, scope))
      {
        var disposalOrder = (int)((ConstantExpression)args[5]).Value;
        if (disposalOrder == 0)
          scope.AddUnorderedDisposable(disp);
        else
          scope.AddDisposable(disp, disposalOrder);
      }

      return result;
    }

    private static object InterpretGetScopedOrSingletonViaFactoryDelegate(IResolverContext r,
        MethodCallExpression callExpr, object paramExprs, object paramValues, ParentLambdaArgs parentArgs, bool useFec)
    {
#if SUPPORTS_FAST_EXPRESSION_COMPILER
      var fewArgExpr = (FourArgumentsMethodCallExpression)callExpr;
      var resolverArg = fewArgExpr.Argument0;
#else
            var args = callExpr.Arguments.ToListOrSelf();
            var resolverArg = args[0];
#endif
      if (!ReferenceEquals(resolverArg, FactoryDelegateCompiler.ResolverContextParamExpr))
      {
        if (!TryInterpret(r, resolverArg, paramExprs, paramValues, parentArgs, useFec, out var resolverObj))
          return false;
        r = (IResolverContext)resolverObj;
      }

      var scope = (Scope)(r.CurrentScope ?? r.SingletonScope);

#if SUPPORTS_FAST_EXPRESSION_COMPILER
      var factoryIdArg = fewArgExpr.Argument1;
#else
            var factoryIdArg = args[1];
#endif
      var id = (int)((ConstantExpression)factoryIdArg).Value;

      ref var map = ref scope._maps[id & Scope.MAP_COUNT_SUFFIX_MASK];
      var itemRef = map.GetEntryOrDefault(id);
      if (itemRef != null && itemRef.Value != Scope.NoItem)
        return itemRef.Value;

      if (scope.IsDisposed)
        Throw.It(Error.ScopeIsDisposed, scope.ToString());

      itemRef = new ImMapEntry<object>(id, Scope.NoItem);
      var oldMap = map;
      var newMap = oldMap.AddOrKeepEntry(itemRef);
      if (Interlocked.CompareExchange(ref map, newMap, oldMap) != oldMap)
      {
        newMap = Ref.SwapAndGetNewValue(ref map, itemRef, (x, i) => x.AddOrKeepEntry(i));
        var otherItemRef = newMap.GetSurePresentEntry(id);
        if (otherItemRef != itemRef)
          return otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
      }
      else if (newMap == oldMap)
      {
        var otherItemRef = newMap.GetSurePresentEntry(id);
        return otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
      }

#if SUPPORTS_FAST_EXPRESSION_COMPILER
      var lambda = fewArgExpr.Argument2;
#else
            var lambda = args[2];
#endif
      object result = null;
#if SUPPORTS_SPIN_WAIT
      if (lambda is ConstantExpression lambdaConstExpr)
        result = ((FactoryDelegate)lambdaConstExpr.Value)(r);
      else if (!TryInterpret(r, ((LambdaExpression)lambda).Body, paramExprs, paramValues, parentArgs, useFec, out result))
        result = ((LambdaExpression)lambda).Body.CompileToFactoryDelegate(useFec, ((IContainer)r).Rules.UseInterpretation)(r);
      itemRef.Value = result;
#else
            lock (itemRef) 
            {
                if (lambda is ConstantExpression lambdaConstExpr)
                    result = ((FactoryDelegate)lambdaConstExpr.Value)(r);
                else if (!TryInterpret(r, ((LambdaExpression)lambda).Body, paramExprs, paramValues, parentArgs, useFec, out result))
                    result = ((LambdaExpression)lambda).Body.CompileToFactoryDelegate(useFec, ((IContainer)r).Rules.UseInterpretation)(r);
                
                itemRef.Value = result;
                Monitor.PulseAll(itemRef);
            }
#endif
      if (result is IDisposable disp && !ReferenceEquals(disp, scope))
      {
#if SUPPORTS_FAST_EXPRESSION_COMPILER
        var disposalOrderArg = fewArgExpr.Argument3;
#else
                var disposalOrderArg = args[3];
#endif
        var disposalOrder = (int)((ConstantExpression)disposalOrderArg).Value;
        if (disposalOrder == 0)
          scope.AddUnorderedDisposable(disp);
        else
          scope.AddDisposable(disp, disposalOrder);
      }

      return result;
    }

    [MethodImpl((MethodImplOptions)256)]
    private static object ConstValue(Expression expr) => ((ConstantExpression)expr).Value;
  }

  internal static class Converter
  {
    public static object ConvertWithOperator(object source, Type targetType, Expression expr)
    {
      var sourceType = source.GetType();
      var sourceConvertOp = sourceType.FindConvertOperator(sourceType, targetType);
      if (sourceConvertOp != null)
        return sourceConvertOp.Invoke(null, new[] { source });

      var targetConvertOp = targetType.FindConvertOperator(sourceType, targetType);
      if (targetConvertOp == null)
        Throw.It(Error.NoConversionOperatorFoundWhenInterpretingTheConvertExpression, source, targetType, expr);
      return targetConvertOp.Invoke(null, new[] { source });
    }

    public static object ConvertMany(object[] source, Type targetType) =>
        _convertManyMethod.MakeGenericMethod(targetType).Invoke(null, source.One());

    public static R[] DoConvertMany<R>(object[] items)
    {
      if (items == null && items.Length == 0)
        return ArrayTools.Empty<R>();

      var results = new R[items.Length];
      for (var i = 0; i < items.Length; i++)
        results[i] = (R)items[i];
      return results;
    }

    private static readonly MethodInfo _convertManyMethod =
        typeof(Converter).GetTypeInfo().GetDeclaredMethod(nameof(DoConvertMany));
  }

  /// <summary>Compiles expression to factory delegate.</summary>
  public static class FactoryDelegateCompiler
  {
    /// <summary>Resolver context parameter expression in FactoryDelegate.</summary>
    public static readonly ParameterExpression ResolverContextParamExpr = Parameter(typeof(IResolverContext), "r");

    /// [Obsolete("Not used anymore")]
    public static readonly Type[] FactoryDelegateParamTypes = { typeof(IResolverContext) };

    /// Optimization: singleton array with the parameter expression of IResolverContext
    public static readonly ParameterExpression[] FactoryDelegateParamExprs = { ResolverContextParamExpr };

    /// Strips the unnecessary or adds the necessary cast to expression return result
    public static Expression NormalizeExpression(this Expression expr)
    {
      // System.Linq.Expressions.ExpressionType is used by FEC as well 
      if (expr.NodeType == System.Linq.Expressions.ExpressionType.Convert)
      {
        var operandExpr = ((UnaryExpression)expr).Operand;
        if (operandExpr.Type == typeof(object))
          return operandExpr;
      }

      if (expr.Type != typeof(void) && expr.Type.IsValueType())
        return Convert(expr, typeof(object));

      return expr;
    }

    /// <summary>Wraps service creation expression (body) into <see cref="FactoryDelegate"/> and returns result lambda expression.</summary>
    public static Expression<FactoryDelegate> WrapInFactoryExpression(this Expression expression) =>
        Lambda<FactoryDelegate>(expression.NormalizeExpression(), FactoryDelegateParamExprs
#if SUPPORTS_FAST_EXPRESSION_COMPILER
                , typeof(object)
#endif
                );

    /// <summary>First wraps the input service expression into lambda expression and
    /// then compiles lambda expression to actual <see cref="FactoryDelegate"/> used for service resolution.</summary>
    public static FactoryDelegate CompileToFactoryDelegate(
        this Expression expression, bool useFastExpressionCompiler, bool preferInterpretation)
    {
      expression = expression.NormalizeExpression();
      if (expression is ConstantExpression constExpr)
        return constExpr.Value.ToFactoryDelegate;

      if (!preferInterpretation && useFastExpressionCompiler)
      {
        var factoryDelegate = (FactoryDelegate)(FastExpressionCompiler.LightExpression.ExpressionCompiler.TryCompileBoundToFirstClosureParam(
            typeof(FactoryDelegate), expression, FactoryDelegateParamExprs,
            new[] { typeof(FastExpressionCompiler.LightExpression.ExpressionCompiler.ArrayClosure), typeof(IResolverContext) }, typeof(object)));
        if (factoryDelegate != null)
          return factoryDelegate;
      }

      // fallback for platforms when FastExpressionCompiler is not supported,
      // or just in case when some expression is not supported (did not found one yet)
#if SUPPORTS_FAST_EXPRESSION_COMPILER
      var lambda = Lambda<FactoryDelegate>(expression, FactoryDelegateParamExprs, typeof(object))
          .ToLambdaExpression();
#else
            var lambda = Lambda<FactoryDelegate>(expression, FactoryDelegateParamExprs);
#endif

      if (preferInterpretation)
      {
#if SUPPORTS_EXPRESSION_COMPILE_WITH_PREFER_INTERPRETATION_PARAM
        return lambda.Compile(preferInterpretation: true);
#else
                return lambda.Compile();
#endif
      }

      return lambda.Compile();
    }

    /// <summary>Compiles lambda expression to actual `FactoryDelegate` wrapper.</summary>
    public static object CompileToFactoryDelegate(this Expression expression,
        Type factoryDelegateType, Type resultType, bool useFastExpressionCompiler, bool preferInterpretation)
    {
      if (!preferInterpretation && useFastExpressionCompiler)
      {
        var factoryDelegate = (FastExpressionCompiler.LightExpression.ExpressionCompiler.TryCompileBoundToFirstClosureParam(
            factoryDelegateType, expression, FactoryDelegateParamExprs,
            new[] { typeof(FastExpressionCompiler.LightExpression.ExpressionCompiler.ArrayClosure), typeof(IResolverContext) }, resultType));
        if (factoryDelegate != null)
          return factoryDelegate;
      }

      // fallback for platforms when FastExpressionCompiler is not supported,
      // or just in case when some expression is not supported (did not found one yet)
#if SUPPORTS_FAST_EXPRESSION_COMPILER
      return Lambda(factoryDelegateType, expression, FactoryDelegateParamExprs, resultType).ToLambdaExpression()
#else
            return Lambda(factoryDelegateType, expression, FactoryDelegateParamExprs)
#endif
                .Compile(
#if SUPPORTS_EXPRESSION_COMPILE_WITH_PREFER_INTERPRETATION_PARAM
                    preferInterpretation
#endif
                );
    }

    /// [Obsolete("Use the version with `preferInterpretation` parameter instead")]
    public static FactoryDelegate CompileToFactoryDelegate(this Expression expression,
        bool useFastExpressionCompiler = false)
    {
      expression = expression.NormalizeExpression();

      // Optimization for constants
      if (expression is ConstantExpression ce)
        return ce.Value.ToFactoryDelegate;

      if (useFastExpressionCompiler)
      {
        var factoryDelegate = (FactoryDelegate)(FastExpressionCompiler.LightExpression.ExpressionCompiler.TryCompileBoundToFirstClosureParam(
            typeof(FactoryDelegate), expression, FactoryDelegateParamExprs,
            new[] { typeof(FastExpressionCompiler.LightExpression.ExpressionCompiler.ArrayClosure), typeof(IResolverContext) }, typeof(object)));

        if (factoryDelegate != null)
          return factoryDelegate;
      }

      // fallback for platforms when FastExpressionCompiler is not supported,
      // or just in case when some expression is not supported (did not found one yet)
#if SUPPORTS_FAST_EXPRESSION_COMPILER
      return Lambda<FactoryDelegate>(expression, FactoryDelegateParamExprs, typeof(object)).ToLambdaExpression().Compile();
#else
            return Lambda<FactoryDelegate>(expression, FactoryDelegateParamExprs).Compile();
#endif
    }

    // todo: remove unused
    /// <summary>Restores the expression from LightExpression, or returns itself if already an Expression.</summary>
    public static System.Linq.Expressions.Expression ToExpression(this Expression expr) =>
#if SUPPORTS_FAST_EXPRESSION_COMPILER
            expr.ToExpression();
#else
            expr;
#endif
  }

  /// <summary>Container extended features.</summary>
  public static class ContainerTools
  {
    /// <summary>The default key for services registered into container created by <see cref="CreateFacade"/></summary>
    public const string FacadeKey = "@facade";

    /// <summary>Uses the provided or the default <see cref="FacadeKey" /> to tweak the rules
    /// to use with the `CreateFacade` methods</summary>
    public static Rules WithFacadeRules(this Rules rules, string facadeKey = FacadeKey) =>
        rules.WithDefaultRegistrationServiceKey(facadeKey)
             .WithFactorySelector(Rules.SelectKeyedOverDefaultFactory(facadeKey));

    /// <summary>Allows to register new specially keyed services which will facade the same default service,
    /// registered earlier. May be used to "override" registrations when testing the container.
    /// Facade will clone the source container singleton and open scope (if any) so
    /// that you may safely disposing the facade without disposing the source container scopes.</summary>
    public static IContainer CreateFacade(this IContainer container, string facadeKey = FacadeKey) =>
        container.CreateChild(newRules: container.Rules.WithFacadeRules(facadeKey));

    /// <summary>The "child" container detached from the parent:
    /// Child creation has O(1) cost - it is cheap thanks to the fast immutable collections cloning.
    /// Child has all parent registrations copied, then the registrations added or removed in the child are not affecting the parent.
    /// By default child will use the parent <see cref="IfAlreadyRegistered"/> policy - you may specify `IfAlreadyRegistered.Replace` to "shadow" the parent registrations
    /// Child has an access to the scoped services and singletons already created by parent.
    /// Child can be disposed without affecting the parent, disposing the child will dispose only the scoped services and singletons created in the child and not in the parent (can be opt-out)</summary>
    public static IContainer CreateChild(this IContainer container,
        IfAlreadyRegistered? ifAlreadyRegistered = null, Rules newRules = null, bool withDisposables = false)
    {
      var rules = newRules != null && newRules != container.Rules ? newRules : container.Rules;
      return container.With(
          container.Parent,
          ifAlreadyRegistered == null ? rules : rules.WithDefaultIfAlreadyRegistered(ifAlreadyRegistered.Value),
          container.ScopeContext,
          RegistrySharing.CloneAndDropCache,
          container.SingletonScope.Clone(withDisposables),
          container.CurrentScope?.Clone(withDisposables));
    }

    /// <summary>Shares all of container state except the cache and the new rules.</summary>
    public static IContainer With(this IContainer container,
        Func<Rules, Rules> configure = null, IScopeContext scopeContext = null) =>
        container.With(
            configure?.Invoke(container.Rules) ?? container.Rules,
            scopeContext ?? container.ScopeContext,
            RegistrySharing.CloneAndDropCache,
            container.SingletonScope);

    /// <summary>Prepares container for expression generation.</summary>
    public static IContainer WithExpressionGeneration(this IContainer container, bool allowRuntimeState = false) =>
        container.With(rules => rules.WithExpressionGeneration(allowRuntimeState));

    /// <summary>Returns new container with all expression, delegate, items cache removed/reset.
    /// But it will preserve resolved services in Singleton/Current scope.</summary>
    public static IContainer WithoutCache(this IContainer container) =>
        container.With(container.Rules, container.ScopeContext,
            RegistrySharing.CloneAndDropCache, container.SingletonScope);

    /// <summary>Creates new container with state shared with original, except for the singletons and cache.</summary>
    public static IContainer WithoutSingletonsAndCache(this IContainer container) =>
        container.With(container.Rules, container.ScopeContext,
            RegistrySharing.CloneAndDropCache, singletonScope: null);

    /// <summary>Shares the setup with original container but copies the registrations, so the new registrations
    /// won't be visible in original. Registrations include decorators and wrappers as well.</summary>
    public static IContainer WithRegistrationsCopy(this IContainer container, bool preserveCache = false) =>
        container.With(container.Rules, container.ScopeContext,
            preserveCache ? RegistrySharing.CloneButKeepCache : RegistrySharing.CloneAndDropCache,
            container.SingletonScope);

    /// <summary>Shares the setup with original container but copies the registrations, so the new registrations
    /// won't be visible in original. Registrations include decorators and wrappers as well.
    /// You may control <see cref="IsRegistryChangePermitted" /> behavior and opt-in for the keeping or cloning the cache.</summary>
    public static IContainer WithRegistrationsCopy(this IContainer container, IsRegistryChangePermitted isRegistryChangePermitted,
        bool preserveCache = false) =>
        container.With(container.Parent, container.Rules, container.ScopeContext,
            preserveCache ? RegistrySharing.CloneButKeepCache : RegistrySharing.CloneAndDropCache,
            container.SingletonScope, container.OwnCurrentScope, isRegistryChangePermitted);

    /// <summary>For given instance resolves and sets properties and fields.
    /// It respects <see cref="Rules.PropertiesAndFields"/> rules set per container,
    /// or if rules are not set it uses <see cref="PropertiesAndFields.Auto"/>.</summary>
    public static TService InjectPropertiesAndFields<TService>(this IResolverContext r, TService instance) =>
        r.InjectPropertiesAndFields<TService>(instance, null);

    /// <summary>For given instance resolves and sets properties and fields. You may specify what 
    /// properties and fields.</summary>
    public static TService InjectPropertiesAndFields<TService>(this IResolverContext r, TService instance,
        params string[] propertyAndFieldNames)
    {
      r.InjectPropertiesAndFields(instance, propertyAndFieldNames);
      return instance;
    }

    // todo: @bug does it OK to share the singletons though despite the promise of not affecting the original container?
    /// <summary>Creates service using container for injecting parameters without registering anything in <paramref name="container"/> if the TYPE is not registered yet. 
    /// The note is that container will share the singletons though.</summary>
    public static object New(this IContainer container, Type concreteType, Setup setup, Made made = null,
        RegistrySharing registrySharing = RegistrySharing.CloneButKeepCache)
    {
      var containerClone = container.With(
          container.Parent, container.Rules, container.ScopeContext,
          registrySharing,
          container.SingletonScope, container.OwnCurrentScope, // reusing the singletons and scopes
          null);

      var implType = containerClone.GetWrappedType(concreteType, null);

      var condition = setup == null && made == null ? null
          : made == null ? (Func<Factory, bool>)(f => f.Setup == setup)
          : setup == null ? (Func<Factory, bool>)(f => f.Made == made)
          : (f => f.Made == made && f.Setup == setup);

      if (!containerClone.IsRegistered(implType, condition: condition))
        containerClone.Register(implType, made: made, setup: setup);

      // No need to Dispose facade because it shares singleton/open scopes with source container, and disposing source container does the job.
      return containerClone.Resolve(concreteType, IfUnresolved.Throw);
    }

    /// <summary>Creates service using container for injecting parameters without registering anything in <paramref name="container"/>.</summary>
    /// <param name="container">Container to use for type creation and injecting its dependencies.</param>
    /// <param name="concreteType">Type to instantiate. Wrappers (Func, Lazy, etc.) is also supported.</param>
    /// <param name="made">(optional) Injection rules to select constructor/factory method, inject parameters, 
    /// properties and fields.</param>
    /// <param name="registrySharing">The default is <see cref="RegistrySharing.CloneButKeepCache"/></param>
    /// <returns>Object instantiated by constructor or object returned by factory method.</returns>
    public static object New(this IContainer container, Type concreteType, Made made = null,
        RegistrySharing registrySharing = RegistrySharing.CloneButKeepCache) =>
        container.New(concreteType, setup: null, made, registrySharing);

    /// <summary>Creates service using container for injecting parameters without registering anything in <paramref name="container"/>.</summary>
    /// <typeparam name="T">Type to instantiate.</typeparam>
    /// <param name="container">Container to use for type creation and injecting its dependencies.</param>
    /// <param name="made">(optional) Injection rules to select constructor/factory method, inject parameters, properties and fields.</param>
    /// <param name="registrySharing">The default is <see cref="RegistrySharing.CloneButKeepCache"/></param>
    /// <returns>Object instantiated by constructor or object returned by factory method.</returns>
    public static T New<T>(this IContainer container, Made made = null,
        RegistrySharing registrySharing = RegistrySharing.CloneButKeepCache) =>
        (T)container.New(typeof(T), made, registrySharing);

    /// <summary>Creates service given strongly-typed creation expression.
    /// Can be used to invoke arbitrary method returning some value with injecting its parameters from container.</summary>
    /// <typeparam name="T">Method or constructor result type.</typeparam>
    /// <param name="container">Container to use for injecting dependencies.</param>
    /// <param name="made">Creation expression.</param>
    /// <param name="registrySharing">The default is <see cref="RegistrySharing.CloneButKeepCache"/></param>
    /// <returns>Created result.</returns>
    public static T New<T>(this IContainer container, Made.TypedMade<T> made,
        RegistrySharing registrySharing = RegistrySharing.CloneButKeepCache) =>
        (T)container.New(typeof(T), made, registrySharing);

    // todo: vNext: remove, replaced by Registrator.RegisterMapping
    /// <summary>Registers new service type with factory for registered service type.
    /// Throw if no such registered service type in container.</summary>
    /// <param name="container">Container</param> <param name="serviceType">New service type.</param>
    /// <param name="registeredServiceType">Existing registered service type.</param>
    /// <param name="serviceKey">(optional)</param> <param name="registeredServiceKey">(optional)</param>
    public static void RegisterMapping(this IContainer container, Type serviceType, Type registeredServiceType,
        object serviceKey = null, object registeredServiceKey = null) =>
        Registrator.RegisterMapping(container,
            serviceType, registeredServiceType, serviceKey, registeredServiceKey);

    // todo: vNext: remove, replaced by Registrator.RegisterMapping
    /// <summary>Registers new service type with factory for registered service type.
    /// Throw if no such registered service type in container.</summary>
    /// <param name="container">Container</param>
    /// <typeparam name="TService">New service type.</typeparam>
    /// <typeparam name="TRegisteredService">Existing registered service type.</typeparam>
    /// <param name="serviceKey">(optional)</param> <param name="registeredServiceKey">(optional)</param>
    public static void RegisterMapping<TService, TRegisteredService>(this IContainer container,
        object serviceKey = null, object registeredServiceKey = null) =>
        Registrator.RegisterMapping(container,
            typeof(TService), typeof(TRegisteredService), serviceKey, registeredServiceKey);

    // todo: Remove in VNext?
    /// <summary>Register a service without implementation which can be provided later in terms
    /// of normal registration with `IfAlreadyRegistered.Replace` parameter.
    /// When the implementation is still not provided when the placeholder service is accessed, then the exception will be thrown.
    /// This feature allows you to postpone the decision on implementation until it is later known.</summary>
    /// <remarks>Internally the empty factory is registered with the setup `asResolutionCall: true`.
    /// That means, instead of placing service instance into graph expression we put here redirecting call to
    /// container Resolve.</remarks>
    public static void RegisterPlaceholder(this IContainer container, Type serviceType,
        IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        Registrator.RegisterPlaceholder(container, serviceType, ifAlreadyRegistered, serviceKey);

    // todo: vNext: Remove, replaced by Registrator.RegisterPlaceholder
    /// <summary>Register a service without implementation which can be provided later in terms
    /// of normal registration with `IfAlreadyRegistered.Replace` parameter.
    /// When the implementation is still not provided when the placeholder service is accessed, then the exception will be thrown.
    /// This feature allows you to postpone the decision on implementation until it is later known.</summary>
    /// <remarks>Internally the empty factory is registered with the setup `asResolutionCall: true`.
    /// That means, instead of placing service instance into graph expression we put here redirecting call to
    /// container Resolve.</remarks>
    public static void RegisterPlaceholder<TService>(this IContainer container,
        IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        Registrator.RegisterPlaceholder(container, typeof(TService), ifAlreadyRegistered, serviceKey);

    /// Obsolete: please use WithAutoFallbackDynamicRegistration
    [Obsolete("Please use WithAutoFallbackDynamicRegistration instead")]
    public static IContainer WithAutoFallbackResolution(this IContainer container,
        IEnumerable<Type> implTypes,
        Func<IReuse, Request, IReuse> changeDefaultReuse = null,
        Func<Request, bool> condition = null) =>
        container.ThrowIfNull().With(rules =>
            rules.WithUnknownServiceResolvers(
                Rules.AutoRegisterUnknownServiceRule(implTypes, changeDefaultReuse, condition)));

    /// Obsolete: please use WithAutoFallbackDynamicRegistration
    [Obsolete("Please use WithAutoFallbackDynamicRegistration instead")]
    public static IContainer WithAutoFallbackResolution(this IContainer container,
        IEnumerable<Assembly> implTypeAssemblies,
        Func<IReuse, Request, IReuse> changeDefaultReuse = null,
        Func<Request, bool> condition = null) =>
        container.WithAutoFallbackResolution(implTypeAssemblies.ThrowIfNull()
                 .SelectMany(assembly => assembly.GetLoadedTypes())
                 .Where(Registrator.IsImplementationType).ToArray(),
                 changeDefaultReuse, condition);

    /// <summary>Provides automatic fallback resolution mechanism for not normally registered
    /// services. Underneath it uses the `WithDynamicRegistrations`.</summary>
    public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
        Func<Type, object, IEnumerable<Type>> getImplTypes, Func<Type, Factory> factory = null) =>
        container.ThrowIfNull()
            .With(rules => rules.WithDynamicRegistrationsAsFallback(
                Rules.AutoFallbackDynamicRegistrations(getImplTypes, factory)));

    /// <summary>Provides automatic fallback resolution mechanism for not normally registered
    /// services. Underneath it uses the `WithDynamicRegistrations`.</summary>
    public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
        DynamicRegistrationFlags flags,
        Func<Type, object, IEnumerable<Type>> getImplTypes, Func<Type, Factory> factory = null) =>
        container.ThrowIfNull()
            .With(rules => rules.WithDynamicRegistrationsAsFallback(flags,
                Rules.AutoFallbackDynamicRegistrations(getImplTypes, factory)));

    /// <summary>Provides automatic fallback resolution mechanism for not normally registered
    /// services. Underneath it uses the `WithDynamicRegistrations`.</summary>
    public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container, params Type[] implTypes) =>
        container.WithAutoFallbackDynamicRegistrations((_, __) => implTypes);

    /// <summary>Provides automatic fallback resolution mechanism for not normally registered
    /// services. Underneath it uses the `WithDynamicRegistrations`.</summary>
    public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
        IReuse reuse, params Type[] implTypes) =>
        container.WithAutoFallbackDynamicRegistrations((_, __) => implTypes, implType => new ReflectionFactory(implType, reuse));

    /// <summary>Provides automatic fallback resolution mechanism for not normally registered
    /// services. Underneath it uses the `WithDynamicRegistrations`.</summary>
    public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
        IReuse reuse, Setup setup, params Type[] implTypes) =>
        container.WithAutoFallbackDynamicRegistrations(
            (ignoredServiceType, ignoredServiceKey) => implTypes,
            implType => new ReflectionFactory(implType, reuse, setup: setup));

    /// <summary>Provides automatic fallback resolution mechanism for not normally registered
    /// services. Underneath it uses the `WithDynamicRegistrations`.</summary>
    public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
        Func<Type, object, IEnumerable<Assembly>> getImplTypeAssemblies, Func<Type, Factory> factory = null) =>
        container.WithAutoFallbackDynamicRegistrations(Rules.DefaultDynamicRegistrationFlags, getImplTypeAssemblies, factory);

    /// <summary>Provides automatic fallback resolution mechanism for not normally registered
    /// services. Underneath it uses the `WithDynamicRegistrations`.</summary>
    public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
        DynamicRegistrationFlags flags,
        Func<Type, object, IEnumerable<Assembly>> getImplTypeAssemblies,
        Func<Type, Factory> factory = null) =>
        container.ThrowIfNull().With(rules => rules.WithDynamicRegistrationsAsFallback(
            flags,
            Rules.AutoFallbackDynamicRegistrations(
                (serviceType, serviceKey) =>
                {
                  var assemblies = getImplTypeAssemblies(serviceType, serviceKey);
                  if (assemblies == null)
                    return Empty<Type>();
                  return assemblies
                          .SelectMany(a => ReflectionTools.GetLoadedTypes(a))
                          .Where(t => Registrator.IsImplementationType(t))
                          .ToArray();
                },
                factory)));

    /// <summary>Provides automatic fallback resolution mechanism for not normally registered
    /// services. Underneath it uses the `WithDynamicRegistrations`.</summary>
    public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
        params Assembly[] implTypeAssemblies) =>
        container.WithAutoFallbackDynamicRegistrations((_, __) => implTypeAssemblies);

    /// <summary>Provides automatic fallback resolution mechanism for not normally registered
    /// services. Underneath it uses the `WithDynamicRegistrations`.</summary>
    public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
        IEnumerable<Assembly> implTypeAssemblies) =>
        container.WithAutoFallbackDynamicRegistrations((_, __) => implTypeAssemblies);

    /// <summary>Creates new container with provided parameters and properties
    /// to pass the custom dependency values for injection. The old parameters and properties are overridden,
    /// but not replaced.</summary>
    /// <param name="container">Container to work with.</param>
    /// <param name="parameters">(optional) Parameters specification, can be used to proved custom values.</param>
    /// <param name="propertiesAndFields">(optional) Properties and fields specification, can be used to proved custom values.</param>
    /// <returns>New container with adjusted rules.</returns>
    /// <example><code lang="cs"><![CDATA[
    ///     var c = container.WithDependencies(Parameters.Of.Type<string>(_ => "Nya!"));
    ///     var a = c.Resolve<A>(); // where A accepts string parameter in constructor
    ///     Assert.AreEqual("Nya!", a.Message)
    /// ]]></code></example>
    public static IContainer WithDependencies(this IContainer container,
        ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null) =>
        container.With(rules => rules.With(Made.Of(
            parameters: rules.Parameters.OverrideWith(parameters),
            propertiesAndFields: rules.PropertiesAndFields.OverrideWith(propertiesAndFields)),
            overrideRegistrationMade: true));

    /// <summary>Result of GenerateResolutionExpressions methods</summary>
    public class GeneratedExpressions
    {
      /// <summary>Resolutions roots</summary>
      public readonly List<KeyValuePair<ServiceInfo, System.Linq.Expressions.Expression<FactoryDelegate>>>
          Roots = new List<KeyValuePair<ServiceInfo, System.Linq.Expressions.Expression<FactoryDelegate>>>();

      /// <summary>Dependency of Resolve calls</summary>
      public readonly List<KeyValuePair<Request, System.Linq.Expressions.Expression>>
          ResolveDependencies = new List<KeyValuePair<Request, System.Linq.Expressions.Expression>>();

      /// <summary>Errors</summary>
      public readonly List<KeyValuePair<ServiceInfo, ContainerException>>
          Errors = new List<KeyValuePair<ServiceInfo, ContainerException>>();
    }

    /// <summary>Generates expressions for specified roots and their "Resolve-call" dependencies.
    /// Wraps exceptions into errors. The method does not create any actual services.
    /// You may use Factory <see cref="Setup.AsResolutionRoot"/>.</summary>
    public static GeneratedExpressions GenerateResolutionExpressions(this IContainer container,
        Func<IEnumerable<ServiceRegistrationInfo>, IEnumerable<ServiceInfo>> getRoots = null, bool allowRuntimeState = false)
    {
      var generatingContainer = container.WithExpressionGeneration(allowRuntimeState);
      var regs = generatingContainer.GetServiceRegistrations();
      var roots = getRoots != null ? getRoots(regs) : regs.Select(r => r.ToServiceInfo());

      var result = new GeneratedExpressions();
      foreach (var root in roots)
      {
        try
        {
          var request = Request.Create(generatingContainer, root);
          var expr = generatingContainer.ResolveFactory(request)?.GetExpressionOrDefault(request);
          if (expr == null)
            continue;

          result.Roots.Add(root.Pair(expr.WrapInFactoryExpression()
#if SUPPORTS_FAST_EXPRESSION_COMPILER
                        .ToLambdaExpression()
#endif
                    ));
        }
        catch (ContainerException ex)
        {
          result.Errors.Add(root.Pair(ex));
        }
      }

      var depExprs = generatingContainer.Rules.DependencyResolutionCallExprs.Value;
      result.ResolveDependencies.AddRange(depExprs.Enumerate().Select(r => r.Key.Pair(r.Value)));
      return result;
    }

    /// <summary>Generates expressions for provided root services</summary>
    public static GeneratedExpressions GenerateResolutionExpressions(
        this IContainer container, Func<ServiceRegistrationInfo, bool> condition) =>
        container.GenerateResolutionExpressions(regs => regs.Where(condition.ThrowIfNull()).Select(r => r.ToServiceInfo()));

    /// <summary>Generates expressions for provided root services</summary>
    public static GeneratedExpressions GenerateResolutionExpressions(
        this IContainer container, params ServiceInfo[] roots) =>
        container.GenerateResolutionExpressions(roots.ToFunc<IEnumerable<ServiceRegistrationInfo>, IEnumerable<ServiceInfo>>);

    /// <summary>Excluding open-generic registrations, cause you need to provide type arguments to actually create these types.</summary>
    public static bool DefaultValidateCondition(ServiceRegistrationInfo reg) => !reg.ServiceType.IsOpenGeneric();

    /// <summary>Helps to find potential problems in service registration setup. Method tries to resolve the specified registrations, collects exceptions, 
    /// and returns them to user. Does not create any actual service objects. You must specify <paramref name="condition"/> to define your resolution roots,
    /// otherwise container will try to resolve all registrations, which usually is not realistic case to validate.</summary>
    public static KeyValuePair<ServiceInfo, ContainerException>[] Validate(this IContainer container, Func<ServiceRegistrationInfo, bool> condition = null)
    {
      var noOpenGenericsWithCondition = condition == null
          ? (Func<ServiceRegistrationInfo, bool>)DefaultValidateCondition
          : (r => condition(r) && DefaultValidateCondition(r));

      var roots = container.GetServiceRegistrations().Where(noOpenGenericsWithCondition).Select(r => r.ToServiceInfo()).ToArray();
      if (roots.Length == 0)
        Throw.It(Error.FoundNoRootsToValidate, container);

      return container.Validate(roots);
    }

    /// <summary>Same as the Validate with the same parameters but throws the exception with all collected errors</summary>
    public static void ValidateAndThrow(this IContainer container, Func<ServiceRegistrationInfo, bool> condition = null)
    {
      var errors = container.Validate(condition);
      if (!errors.IsNullOrEmpty())
        Throw.Many(Error.ValidateFoundErrors, errors.Map(x => x.Value));
    }

    /// <summary>Helps to find potential problems when resolving the <paramref name="roots"/>.
    /// Method will collect the exceptions when resolving or injecting the specific root. Does not create any actual service objects.
    /// You must specify <paramref name="roots"/> to define your resolution roots, otherwise container will try to resolve all registrations, 
    /// which usually is not realistic case to validate.</summary>
    public static KeyValuePair<ServiceInfo, ContainerException>[] Validate(this IContainer container, params ServiceInfo[] roots)
    {
      var validatingContainer = container.With(rules => rules.ForValidate());

      var stack = RequestStack.Create(16);

      List<KeyValuePair<ServiceInfo, ContainerException>> errors = null;
      for (var i = 0; i < roots.Length; i++)
      {
        var root = roots[i];
        try
        {
          var request = Request.CreateForValidation(validatingContainer, root, stack);
          validatingContainer.ResolveFactory(request)?.GetExpressionOrDefault(request);
        }
        catch (ContainerException ex)
        {
          if (errors == null)
            errors = new List<KeyValuePair<ServiceInfo, ContainerException>>();
          errors.Add(root.Pair(ex));
        }
      }

      return errors?.ToArray() ?? ArrayTools.Empty<KeyValuePair<ServiceInfo, ContainerException>>();
    }

    /// <summary>Same as the Validate with the same parameters but throws the exception with all collected errors</summary>
    public static void ValidateAndThrow(this IContainer container, params ServiceInfo[] roots)
    {
      var errors = container.Validate(roots);
      if (!errors.IsNullOrEmpty())
        Throw.Many(Error.ValidateFoundErrors, errors.Map(x => x.Value));
    }

    /// <summary>Helps to find potential problems in service registration setup by trying to resolve the <paramref name="serviceTypes"/> and 
    /// returning the found errors. This method does not throw.</summary>
    public static KeyValuePair<ServiceInfo, ContainerException>[] Validate(this IContainer container, params Type[] serviceTypes)
    {
      if (serviceTypes.IsNullOrEmpty())
        Throw.It(Error.NoServiceTypesToValidate, container);
      return container.Validate(serviceTypes.Map(t => ServiceInfo.Of(t)));
    }

    /// <summary>Same as the Validate with the same parameters but throws the exception with all collected errors</summary>
    public static void ValidateAndThrow(this IContainer container, params Type[] serviceTypes)
    {
      var errors = container.Validate(serviceTypes);
      if (!errors.IsNullOrEmpty())
        Throw.Many(Error.ValidateFoundErrors, errors.Map(x => x.Value));
    }

    /// <summary>Re-constructs the whole request chain as request creation expression.</summary>
    public static Expression GetRequestExpression(this IContainer container, Request request,
        RequestFlags requestParentFlags = default(RequestFlags))
    {
      if (request.IsEmpty)
        return (requestParentFlags & RequestFlags.OpensResolutionScope) != 0
            ? Request.EmptyOpensResolutionScopeRequestExpr
            : Request.EmptyRequestExpr;

      var flags = request.Flags | requestParentFlags;
      var r = requestParentFlags == default(RequestFlags) ? request : request.WithFlags(flags);

      // When not for generation, using run-time request object to Minimize generated object graph.
      if (!container.Rules.UsedForExpressionGeneration)
        return Constant(r.IsolateRequestChain());

      // recursively ask for parent expression until it is empty
      var parentExpr = container.GetRequestExpression(request.DirectParent);

      var serviceType = r.ServiceType;
      var ifUnresolved = r.IfUnresolved;
      var requiredServiceType = r.RequiredServiceType;
      var serviceKey = r.ServiceKey;

      var metadataKey = r.MetadataKey;
      var metadata = r.Metadata;

      var factoryID = r.FactoryID;
      var factoryType = r.FactoryType;
      var implementationType = r.ImplementationType;
      var decoratedFactoryID = r.DecoratedFactoryID;

      var serviceTypeExpr = Constant(serviceType);
      var factoryIdExpr = Constant(factoryID);
      var implTypeExpr = Constant(implementationType);
      var reuseExpr = r.Reuse == null ? Constant(null, typeof(IReuse))
          : r.Reuse.ToExpression(it => container.GetConstantExpression(it));

      if (ifUnresolved == IfUnresolved.Throw &&
          requiredServiceType == null && serviceKey == null && metadataKey == null && metadata == null &&
          factoryType == FactoryType.Service && flags == default(RequestFlags) && decoratedFactoryID == 0)
        return Call(parentExpr, Request.PushMethodWith4Args.Value,
            serviceTypeExpr, factoryIdExpr, implTypeExpr, reuseExpr);

      var requiredServiceTypeExpr = Constant(requiredServiceType);
      var serviceKeyExpr = container.GetConstantExpression(serviceKey, typeof(object));
      var factoryTypeExpr = Constant(factoryType);
      var flagsExpr = Constant(flags);

      if (ifUnresolved == IfUnresolved.Throw &&
          metadataKey == null && metadata == null && decoratedFactoryID == 0)
        return Call(parentExpr, Request.PushMethodWith8Args.Value,
            serviceTypeExpr, requiredServiceTypeExpr, serviceKeyExpr,
            factoryIdExpr, factoryTypeExpr, implTypeExpr, reuseExpr, flagsExpr);

      var ifUnresolvedExpr = Constant(ifUnresolved);
      var decoratedFactoryIDExpr = Constant(decoratedFactoryID);

      if (metadataKey == null && metadata == null)
        return Call(parentExpr, Request.PushMethodWith10Args.Value,
            serviceTypeExpr, requiredServiceTypeExpr, serviceKeyExpr, ifUnresolvedExpr,
            factoryIdExpr, factoryTypeExpr, implTypeExpr, reuseExpr, flagsExpr, decoratedFactoryIDExpr);

      var metadataKeyExpr = Constant(metadataKey);
      var metadataExpr = container.GetConstantExpression(metadata, typeof(object));

      return Call(parentExpr, Request.PushMethodWith12Args.Value,
          serviceTypeExpr, requiredServiceTypeExpr, serviceKeyExpr, metadataKeyExpr, metadataExpr, ifUnresolvedExpr,
          factoryIdExpr, factoryTypeExpr, implTypeExpr, reuseExpr, flagsExpr, decoratedFactoryIDExpr);
    }

    /// <summary>Clears delegate and expression cache for specified <typeparamref name="T"/>.
    /// But does not clear instances of already resolved/created singletons and scoped services!</summary>
    public static bool ClearCache<T>(this IContainer container, FactoryType? factoryType = null, object serviceKey = null) =>
        container.ClearCache(typeof(T), factoryType, serviceKey);

    /// <summary>Clears delegate and expression cache for specified service.
    /// But does not clear instances of already resolved/created singletons and scoped services!</summary>
    public static bool ClearCache(this IContainer container, Type serviceType,
        FactoryType? factoryType = null, object serviceKey = null) =>
        container.ClearCache(serviceType, factoryType, serviceKey);
  }

  /// <summary>Interface used to convert reuse instance to expression.</summary>
  public interface IConvertibleToExpression
  {
    /// <summary>Returns expression representation without closure.
    /// Use <paramref name="fallbackConverter"/> to converting the sub-items, constants to container.</summary>
    Expression ToExpression(Func<object, Expression> fallbackConverter);
  }

  /// <summary>Used to represent multiple default service keys.
  /// Exposes <see cref="RegistrationOrder"/> to determine order of service added.</summary>
  public sealed class DefaultKey : IConvertibleToExpression
  {
    /// <summary>Default value.</summary>
    public static readonly DefaultKey Value = new DefaultKey(0);

    /// <summary>Allows to determine service registration order.</summary>
    public readonly int RegistrationOrder;

    /// <summary>Returns the default key with specified registration order.</summary>
    public static DefaultKey Of(int registrationOrder) =>
        registrationOrder == 0 ? Value : new DefaultKey(registrationOrder);

    private static readonly MethodInfo _ofMethod =
        typeof(DefaultKey).GetTypeInfo().GetDeclaredMethod(nameof(Of));

    /// <summary>Converts to expression</summary>
    public Expression ToExpression(Func<object, Expression> fallbackConverter) =>
        Call(_ofMethod, Constant(RegistrationOrder));

    /// <summary>Returns next default key with increased <see cref="RegistrationOrder"/>.</summary>
    public DefaultKey Next() => Of(RegistrationOrder + 1);

    /// <summary>Compares keys based on registration order. The null (represents default) key is considered equal.</summary>
    public override bool Equals(object key) =>
        key == null || (key as DefaultKey)?.RegistrationOrder == RegistrationOrder;

    /// <summary>Returns registration order as hash.</summary>
    public override int GetHashCode() => RegistrationOrder;

    /// <summary>Prints registration order to string.</summary>
    public override string ToString() => GetType().Name + "(" + RegistrationOrder + ")";

    private DefaultKey(int registrationOrder) => RegistrationOrder = registrationOrder;
  }

  /// <summary>Represents default key for dynamic registrations</summary>
  public sealed class DefaultDynamicKey : IConvertibleToExpression
  {
    /// <summary>Default value.</summary>
    public static readonly DefaultDynamicKey Value = new DefaultDynamicKey(0);

    /// <summary>Associated ID.</summary>
    public readonly int RegistrationOrder;

    /// <summary>Returns dynamic key with specified ID.</summary>
    public static DefaultDynamicKey Of(int registrationOrder) =>
        registrationOrder == 0 ? Value : new DefaultDynamicKey(registrationOrder);

    private static readonly MethodInfo _ofMethod =
        typeof(DefaultDynamicKey).GetTypeInfo().GetDeclaredMethod(nameof(Of));

    /// <summary>Converts to expression</summary>
    public Expression ToExpression(Func<object, Expression> fallbackConverter) =>
        Call(_ofMethod, Constant(RegistrationOrder));

    /// <summary>Returns next dynamic key with increased <see cref="RegistrationOrder"/>.</summary> 
    public DefaultDynamicKey Next() => Of(RegistrationOrder + 1);

    /// <summary>Compares key's IDs. The null (default) key is considered equal!</summary>
    public override bool Equals(object key) =>
        key == null || (key as DefaultDynamicKey)?.RegistrationOrder == RegistrationOrder;

    /// <summary>Returns key index as hash.</summary>
    public override int GetHashCode() => RegistrationOrder;

    /// <summary>Prints registration order to string.</summary>
    public override string ToString() => GetType().Name + "(" + RegistrationOrder + ")";

    private DefaultDynamicKey(int registrationOrder)
    {
      RegistrationOrder = registrationOrder;
    }
  }

  /// <summary>Extends IResolver to provide an access to scope hierarchy.</summary>
  public interface IResolverContext : IResolver, IDisposable
  {
    /// <summary>True if container is disposed.</summary>
    bool IsDisposed { get; }

    /// <summary>Parent context of the scoped context.</summary>
    IResolverContext Parent { get; }

    /// <summary>The root context of the scoped context.</summary>
    IResolverContext Root { get; }

    /// <summary>Singleton scope, always associated with root scope.</summary>
    IScope SingletonScope { get; }

    /// <summary>Optional ambient scope context.</summary>
    IScopeContext ScopeContext { get; }

    /// <summary>Current opened scope. May return the current scope from <see cref="ScopeContext"/> if context is not null.</summary>
    IScope CurrentScope { get; }

    /// Creates the resolver context with specified current Container-OWN scope 
    IResolverContext WithCurrentScope(IScope scope);

    /// Put instance into the current scope or singletons.
    void UseInstance(Type serviceType, object instance, IfAlreadyRegistered IfAlreadyRegistered,
        bool preventDisposal, bool weaklyReferenced, object serviceKey);

    /// Puts instance created via the passed factory on demand into the current or singleton scope
    void Use(Type serviceType, FactoryDelegate factory);

    /// <summary>For given instance resolves and sets properties and fields.</summary>
    void InjectPropertiesAndFields(object instance, string[] propertyAndFieldNames);
  }

  /// <summary>Provides a usable abstractions for <see cref="IResolverContext"/></summary>
  public static class ResolverContext
  {
    /// <summary>Just a sugar that allow to get root or self container.</summary>
    public static IResolverContext RootOrSelf(this IResolverContext r) => r.Root ?? r;

    internal static readonly PropertyInfo ParentProperty =
        typeof(IResolverContext).Property(nameof(IResolverContext.Parent));

    internal static readonly MethodInfo OpenScopeMethod =
        typeof(ResolverContext).GetTypeInfo().GetDeclaredMethod(nameof(OpenScope));

    /// <summary>Returns root or self resolver based on request.</summary>
    public static Expression GetRootOrSelfExpr(Request request) =>
         request.Reuse is CurrentScopeReuse == false &&
         request.DirectParent.IsSingletonOrDependencyOfSingleton &&
        !request.OpensResolutionScope &&
        request.Rules.ThrowIfDependencyHasShorterReuseLifespan
            ? RootOrSelfExpr
            : FactoryDelegateCompiler.ResolverContextParamExpr;

    /// <summary>Resolver context parameter expression in FactoryDelegate.</summary>
    public static readonly Expression ParentExpr =
        Property(FactoryDelegateCompiler.ResolverContextParamExpr, ParentProperty);

    /// <summary>Resolver parameter expression in FactoryDelegate.</summary>
    public static readonly Expression RootOrSelfExpr =
        Call(typeof(ResolverContext).GetTypeInfo().GetDeclaredMethod(nameof(RootOrSelf)),
            FactoryDelegateCompiler.ResolverContextParamExpr);

    /// <summary>Resolver parameter expression in FactoryDelegate.</summary>
    public static readonly Expression SingletonScopeExpr =
        Property(FactoryDelegateCompiler.ResolverContextParamExpr,
            typeof(IResolverContext).Property(nameof(IResolverContext.SingletonScope)));

    /// <summary>Access to scopes in FactoryDelegate.</summary>
    public static readonly Expression CurrentScopeExpr =
        Property(FactoryDelegateCompiler.ResolverContextParamExpr,
            typeof(IResolverContext).Property(nameof(IResolverContext.CurrentScope)));

    /// Indicates that context is scoped - that's is only possible if container is not the Root one and has a Parent context
    public static bool IsScoped(this IResolverContext r) => r.Parent != null;

    /// Provides access to the current scope - may return `null` if ambient scope context has it scope changed in-between 
    public static IScope GetCurrentScope(this IResolverContext r, bool throwIfNotFound) =>
        r.CurrentScope ?? (throwIfNotFound ? Throw.For<IScope>(Error.NoCurrentScope, r) : null);

    /// <summary>Gets current scope matching the <paramref name="name"/></summary>
    public static IScope GetNamedScope(this IResolverContext r, object name, bool throwIfNotFound)
    {
      var currentScope = r.CurrentScope;
      if (currentScope == null)
        return throwIfNotFound ? Throw.For<IScope>(Error.NoCurrentScope, r) : null;

      if (name == null)
        return currentScope;

      if (name is IScopeName scopeName)
      {
        for (var s = currentScope; s != null; s = s.Parent)
          if (scopeName.Match(s.Name))
            return s;
      }
      else
      {
        for (var s = currentScope; s != null; s = s.Parent)
          if (ReferenceEquals(name, s.Name) || name.Equals(s.Name))
            return s;
      }

      return !throwIfNotFound ? null : Throw.For<IScope>(Error.NoMatchedScopeFound, name, currentScope);
    }

    /// <summary>Opens scope with optional name and optional tracking of new scope in a parent scope.</summary>
    /// <param name="r">Parent context to use.</param>
    /// <param name="name">(optional)</param>
    /// <param name="trackInParent">(optional) Instructs to additionally store the opened scope in parent, 
    /// so it will be disposed when parent is disposed. If no parent scope is available the scope will be tracked by Singleton scope.
    /// Used to dispose a resolution scope.</param>
    /// <returns>Scoped resolver context.</returns>
    /// <example><code lang="cs"><![CDATA[
    /// using (var scope = container.OpenScope())
    /// {
    ///     var handler = scope.Resolve<IHandler>();
    ///     handler.Handle(data);
    /// }
    /// ]]></code></example>
    public static IResolverContext OpenScope(this IResolverContext r, object name = null, bool trackInParent = false)
    {
      if (r.ScopeContext == null)
      {
        // todo: may use `r.OwnCurrentScope` when its moved to `IResolverContext` from `IContainer`
        var parentScope = r.CurrentScope;
        var newOwnScope = new Scope(parentScope, name);
        if (trackInParent)
          (parentScope ?? r.SingletonScope).TrackDisposableWithoutDisposalOrder(newOwnScope);
        return r.WithCurrentScope(newOwnScope);
      }

      var newContextScope = name == null
          ? r.ScopeContext.SetCurrent(parent => new Scope(parent))
          : r.ScopeContext.SetCurrent(parent => new Scope(parent, name));

      if (trackInParent)
        (newContextScope.Parent ?? r.SingletonScope).TrackDisposableWithoutDisposalOrder(newContextScope);
      return r.WithCurrentScope(null);
    }

    [MethodImpl((MethodImplOptions)256)]
    internal static bool TryGetUsedInstance(this IResolverContext r, Type serviceType, out object instance)
    {
      instance = null;
      return r.CurrentScope?.TryGetUsedInstance(r, serviceType, out instance) == true
          || r.SingletonScope.TryGetUsedInstance(r, serviceType, out instance);
    }

    // todo: @perf no need to check for IDisposable in TrackDisposable
    /// <summary>A bit if sugar to track disposable in the current scope or in the singleton scope as a fallback</summary>
    public static T TrackDisposable<T>(this IResolverContext r, T instance) where T : IDisposable =>
        (T)(r.CurrentScope ?? r.SingletonScope).TrackDisposableWithoutDisposalOrder(instance);
  }

  /// <summary>The result delegate generated by DryIoc for service creation.</summary>
  public delegate object FactoryDelegate(IResolverContext r);

  /// <summary>The stronly typed delegate for service creation registered as a Wrapper.</summary>
  public delegate TService FactoryDelegate<TService>(IResolverContext r);

  /// <summary>Adds to Container support for:
  /// <list type="bullet">
  /// <item>Open-generic services</item>
  /// <item>Service generics wrappers and arrays using <see cref="Rules.UnknownServiceResolvers"/> extension point.
  /// Supported wrappers include: Func of <see cref="FuncTypes"/>, Lazy, Many, IEnumerable, arrays, Meta, KeyValuePair, DebugExpression.
  /// All wrapper factories are added into collection of <see cref="Wrappers"/>.
  /// unregistered resolution rule.</item>
  /// </list></summary>
  public static class WrappersSupport
  {
    /// <summary>Supported Func types.</summary>
    public static readonly Type[] FuncTypes =
    {
            typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>),
            typeof(Func<,,,,,>), typeof(Func<,,,,,,>), typeof(Func<,,,,,,,>)
        };

    /// <summary>Supported Action types. Yeah, action I can resolve or inject void returning method as action.</summary>
    public static readonly Type[] ActionTypes =
    {
            typeof(Action), typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), typeof(Action<,,,>),
            typeof(Action<,,,,>), typeof(Action<,,,,,>), typeof(Action<,,,,,,>)
        };

    /// <summary>Supported open-generic collection types - all the interfaces implemented by array.</summary>
    public static readonly Type[] SupportedCollectionTypes =
        typeof(object[]).GetImplementedInterfaces().Match(t => t.IsGeneric(), t => t.GetGenericTypeDefinition());

    /// <summary>Returns true if type is supported <see cref="FuncTypes"/>, and false otherwise.</summary>
    public static bool IsFunc(this Type type)
    {
      if (type.GetTypeInfo().IsGenericType)
      {
        var typeDef = type.GetGenericTypeDefinition();
        var funcTypes = FuncTypes;
        for (var i = 0; i < funcTypes.Length; ++i)
          if (ReferenceEquals(funcTypes[i], typeDef))
            return true;
      }
      return false;
    }

    internal static int CollectionWrapperID { get; private set; }

    /// <summary>Registered wrappers by their concrete or generic definition service type.</summary>
    public static readonly ImMap<ImMap.KValue<Type>> Wrappers = BuildSupportedWrappers();

    private static ImMap<ImMap.KValue<Type>> BuildSupportedWrappers()
    {
      var wrappers = ImMap<ImMap.KValue<Type>>.Empty;

      var arrayExpr = new ExpressionFactory(GetArrayExpression, setup: Setup.Wrapper);
      CollectionWrapperID = arrayExpr.FactoryID;

      var arrayInterfaces = SupportedCollectionTypes;
      for (var i = 0; i < arrayInterfaces.Length; i++)
        wrappers = wrappers.AddOrUpdate(arrayInterfaces[i], arrayExpr);

      wrappers = wrappers.AddOrUpdate(typeof(LazyEnumerable<>),
          new ExpressionFactory(GetLazyEnumerableExpressionOrDefault, setup: Setup.Wrapper));

      wrappers = wrappers.AddOrUpdate(typeof(Lazy<>),
          new ExpressionFactory(r => GetLazyExpressionOrDefault(r), setup: Setup.Wrapper));

      wrappers = wrappers.AddOrUpdate(typeof(KeyValuePair<,>),
          new ExpressionFactory(GetKeyValuePairExpressionOrDefault, setup: Setup.WrapperWith(1)));

      wrappers = wrappers.AddOrUpdate(typeof(Meta<,>),
          new ExpressionFactory(GetMetaExpressionOrDefault, setup: Setup.WrapperWith(0)));

      wrappers = wrappers.AddOrUpdate(typeof(Tuple<,>),
          new ExpressionFactory(GetMetaExpressionOrDefault, setup: Setup.WrapperWith(0)));

      wrappers = wrappers.AddOrUpdate(typeof(System.Linq.Expressions.LambdaExpression),
          new ExpressionFactory(GetLambdaExpressionExpressionOrDefault, setup: Setup.Wrapper));

#if SUPPORTS_FAST_EXPRESSION_COMPILER
      wrappers = wrappers.AddOrUpdate(typeof(FastExpressionCompiler.LightExpression.LambdaExpression),
          new ExpressionFactory(GetFastExpressionCompilerLambdaExpressionExpressionOrDefault, setup: Setup.Wrapper));
#endif

      wrappers = wrappers.AddOrUpdate(typeof(FactoryDelegate),
          new ExpressionFactory(GetFactoryDelegateExpressionOrDefault, setup: Setup.Wrapper));

      wrappers = wrappers.AddOrUpdate(typeof(FactoryDelegate<>),
          new ExpressionFactory(GetFactoryDelegateExpressionOrDefault, setup: Setup.WrapperWith(0)));

      wrappers = wrappers.AddOrUpdate(typeof(Func<>),
          new ExpressionFactory(GetFuncOrActionExpressionOrDefault, setup: Setup.Wrapper));

      for (var i = 0; i < FuncTypes.Length; i++)
        wrappers = wrappers.AddOrUpdate(FuncTypes[i],
            new ExpressionFactory(GetFuncOrActionExpressionOrDefault, setup: Setup.WrapperWith(i)));

      for (var i = 0; i < ActionTypes.Length; i++)
        wrappers = wrappers.AddOrUpdate(ActionTypes[i],
            new ExpressionFactory(GetFuncOrActionExpressionOrDefault,
            setup: Setup.WrapperWith(unwrap: typeof(void).ToFunc<Type, Type>)));

      wrappers = wrappers.AddContainerInterfaces();
      return wrappers;
    }

    private static ImMap<ImMap.KValue<Type>> AddContainerInterfaces(this ImMap<ImMap.KValue<Type>> wrappers)
    {
      var resolverContextExpr = new ExpressionFactory(
          ResolverContext.GetRootOrSelfExpr,
          Reuse.Transient, Setup.WrapperWith(preventDisposal: true));

      var containerExpr = new ExpressionFactory(
          r => Convert(ResolverContext.GetRootOrSelfExpr(r), r.ServiceType),
          Reuse.Transient, Setup.WrapperWith(preventDisposal: true));

      wrappers = wrappers
          .AddOrUpdate(RuntimeHelpers.GetHashCode(typeof(IResolverContext)), typeof(IResolverContext), resolverContextExpr)
          .AddOrUpdate(RuntimeHelpers.GetHashCode(typeof(IResolver)), typeof(IResolver), resolverContextExpr)
          .AddOrUpdate(RuntimeHelpers.GetHashCode(typeof(IContainer)), typeof(IContainer), containerExpr)
          .AddOrUpdate(RuntimeHelpers.GetHashCode(typeof(IRegistrator)), typeof(IRegistrator), containerExpr)
#if SUPPORTS_ISERVICE_PROVIDER
                .AddOrUpdate(RuntimeHelpers.GetHashCode(typeof(IServiceProvider)), typeof(IServiceProvider), resolverContextExpr)
#endif
                ;

      return wrappers;
    }

    internal static readonly MethodInfo ToArrayMethod =
        typeof(ArrayTools).GetTypeInfo().GetDeclaredMethod(nameof(ArrayTools.ToArrayOrSelf));

    private static Expression GetArrayExpression(Request request)
    {
      var collectionType = request.GetActualServiceType();
      var container = request.Container;
      var rules = container.Rules;

      var itemType = collectionType.GetArrayElementTypeOrNull() ?? collectionType.GetGenericParamsAndArgs()[0];

      if (rules.ResolveIEnumerableAsLazyEnumerable)
      {
        var lazyEnumerableExpr = GetLazyEnumerableExpressionOrDefault(request);
        return collectionType.GetGenericDefinitionOrNull() != typeof(IEnumerable<>)
            ? Call(ToArrayMethod.MakeGenericMethod(itemType), lazyEnumerableExpr)
            : lazyEnumerableExpr;
      }

      var details = request._serviceInfo.Details;
      var requiredItemType = container.GetWrappedType(itemType, details.RequiredServiceType);

      var items = container.GetServiceRegisteredAndDynamicFactories(requiredItemType) // todo: @bug check for the unregistered values 
          .Map(requiredItemType, (t, x) => new ServiceRegistrationInfo(x.Value, t, x.Key));

      if (requiredItemType.IsClosedGeneric())
      {
        var requiredItemOpenGenericType = requiredItemType.GetGenericTypeDefinition();
        var openGenericItems = container.GetServiceRegisteredAndDynamicFactories(requiredItemOpenGenericType)  // todo: @bug check for the unregistered values
            .Map(requiredItemOpenGenericType, requiredItemType, (gt, t, f) => new ServiceRegistrationInfo(f.Value, t, new OpenGenericTypeKey(gt, f.Key)));
        items = items.Append(openGenericItems);
      }

      // Append registered generic types with compatible variance,
      // e.g. for IHandler<in E> - IHandler<A> is compatible with IHandler<B> if B : A.
      if (requiredItemType.IsGeneric() && rules.VariantGenericTypesInResolvedCollection)
      {
        var variantGenericItems = container.GetServiceRegistrations().ToArrayOrSelf()
            .Match(requiredItemType, (t, x) => t.IsAssignableVariantGenericTypeFrom(x.ServiceType));
        items = items.Append(variantGenericItems);
      }

      // Composite pattern support: filter out composite parent service skip wrappers and decorators
      var parent = request.Parent;
      if (parent.FactoryType != FactoryType.Service)
        parent = parent.FirstOrDefault(p => p.FactoryType == FactoryType.Service) ?? Request.Empty;

      // check fast for the parent of the same type
      if (!parent.IsEmpty && parent.GetActualServiceType() == requiredItemType)
      {
        items = items.Match(parent.FactoryID, (pID, x) => x.Factory.FactoryID != pID); // todo: @perf replace the Match with the in-place replacement with the `null` without reallocating the arrays
        if (requiredItemType.IsGeneric())
          items = items.Match(parent.FactoryID,
              (pID, x) => x.Factory.FactoryGenerator?.GeneratedFactories.Enumerate().FindFirst(f => f.Value.FactoryID == pID) == null);
      }

      // Return collection of single matched item if key is specified.
      var serviceKey = details.ServiceKey;
      if (serviceKey != null)
        items = items.Match(serviceKey, (key, x) => key.Equals(x.OptionalServiceKey));

      var metadataKey = details.MetadataKey;
      var metadata = details.Metadata;
      if (metadataKey != null || metadata != null)
        items = items.Match(metadataKey.Pair(metadata), (m, x) => x.Factory.Setup.MatchesMetadata(m.Key, m.Value));

      var itemExprs = Empty<Expression>();
      if (!items.IsNullOrEmpty())
      {
        Array.Sort(items); // to resolve the items in order of registration

        for (var i = 0; i < items.Length; i++)
        {
          var item = items[i];
          var itemRequest = request.Push(itemType, item.OptionalServiceKey,
              IfUnresolved.ReturnDefaultIfNotRegistered, requiredServiceType: item.ServiceType);

          var itemExpr = container.ResolveFactory(itemRequest)?.GetExpressionOrDefault(itemRequest);
          if (itemExpr != null)
            itemExprs = itemExprs.Append(itemExpr);
        }
      }

      return NewArrayInit(itemType, itemExprs);
    }

    private static Expression GetLazyEnumerableExpressionOrDefault(Request request)
    {
      var container = request.Container;
      var collectionType = request.ServiceType;
      var itemType = collectionType.GetArrayElementTypeOrNull() ?? collectionType.GetGenericParamsAndArgs()[0];
      var requiredItemType = container.GetWrappedType(itemType, request.RequiredServiceType);

      var resolverExpr = ResolverContext.GetRootOrSelfExpr(request);
      var preResolveParentExpr = container.GetRequestExpression(request);

      var resolveManyExpr = Call(resolverExpr, Resolver.ResolveManyMethod,
          Constant(itemType),
          container.GetConstantExpression(request.ServiceKey),
          Constant(requiredItemType),
          preResolveParentExpr,
          request.GetInputArgsExpr());

      return New(typeof(LazyEnumerable<>).MakeGenericType(itemType)
          .GetTypeInfo().DeclaredConstructors.First(x => x.GetParameters().Length == 1),
          // cast to object is not required cause Resolve already returns IEnumerable<object>
          itemType == typeof(object) ? (Expression)resolveManyExpr : Call(_enumerableCastMethod.MakeGenericMethod(itemType), resolveManyExpr));
    }

    private static readonly MethodInfo _enumerableCastMethod =
        typeof(Enumerable).GetTypeInfo().GetDeclaredMethod(nameof(Enumerable.Cast));

    /// <summary>Gets the expression for <see cref="Lazy{T}"/> wrapper.</summary>
    /// <param name="request">The resolution request.</param>
    /// <param name="nullWrapperForUnresolvedService">if set to <c>true</c> then check for service registration before creating resolution expression.</param>
    /// <returns>Expression: <c><![CDATA[r => new Lazy<TService>(() => r.Resolve{TService}(key, ifUnresolved, requiredType))]]></c></returns>
    public static Expression GetLazyExpressionOrDefault(Request request, bool nullWrapperForUnresolvedService = false)
    {
      var lazyType = request.GetActualServiceType();
      var serviceType = lazyType.GetGenericParamsAndArgs()[0];
      var serviceRequest = request.Push(serviceType);

      var container = request.Container;
      if (!container.Rules.FuncAndLazyWithoutRegistration)
      {
        var serviceFactory = container.ResolveFactory(serviceRequest);
        if (serviceFactory == null)
          return request.IfUnresolved == IfUnresolved.Throw ? null : Constant(null, lazyType);
        serviceRequest = serviceRequest.WithResolvedFactory(serviceFactory, skipRecursiveDependencyCheck: true);
      }

      // creates: r => new Lazy(() => r.Resolve<X>(key))
      // or for singleton : r => new Lazy(() => r.Root.Resolve<X>(key))
      var serviceExpr = Resolver.CreateResolutionExpression(serviceRequest, openResolutionScope: false, asResolutionCall: true);

      // The conversion is required in .NET 3.5 to handle lack of covariance for Func<out T>
      // So that Func<Derived> may be used for Func<Base>
      if (serviceExpr.Type != serviceType &&
          !serviceType.GetTypeInfo().IsAssignableFrom(serviceExpr.Type.GetTypeInfo()))
        serviceExpr = Convert(serviceExpr, serviceType);

      var lazyValueFactoryType = typeof(Func<>).MakeGenericType(serviceType);
      var wrapperCtor = lazyType.Constructor(lazyValueFactoryType);

      return New(wrapperCtor, Lambda(lazyValueFactoryType, serviceExpr, Empty<ParameterExpression>()
#if SUPPORTS_FAST_EXPRESSION_COMPILER
                , serviceType
#endif
                ));
    }

    private static Expression GetFuncOrActionExpressionOrDefault(Request request)
    {
      var wrapperType = request.GetActualServiceType();
      var isAction = wrapperType == typeof(Action);
      if (!isAction)
      {
        var openGenericWrapperType = wrapperType.GetGenericDefinitionOrNull().ThrowIfNull();
        if (FuncTypes.IndexOfReference(openGenericWrapperType) == -1)
          Throw.If(!(isAction = ActionTypes.IndexOfReference(openGenericWrapperType) != -1));
      }

      var argTypes = wrapperType.GetGenericParamsAndArgs();
      var argCount = isAction ? argTypes.Length : argTypes.Length - 1;
      var serviceType = isAction ? typeof(void) : argTypes[argCount];

      var argExprs = Empty<ParameterExpression>(); // may be empty, that's OK
      if (argCount != 0)
      {
        argExprs = new ParameterExpression[argCount];
        for (var i = 0; i < argCount; ++i)
          // assign valid unique argument names for code generation
          argExprs[i] = Parameter(argTypes[i], argTypes[i].Name + "@" + i); // todo: optimize string allocations
        request = request.WithInputArgs(argExprs);
      }

      var serviceRequest = request.Push(serviceType, flags: RequestFlags.IsWrappedInFunc | RequestFlags.IsDirectlyWrappedInFunc);
      var container = request.Container;
      var serviceExpr = container.Rules.FuncAndLazyWithoutRegistration && !isAction
          ? Resolver.CreateResolutionExpression(serviceRequest, openResolutionScope: false, asResolutionCall: true)
          : container.ResolveFactory(serviceRequest)?.GetExpressionOrDefault(serviceRequest);

      if (serviceExpr == null)
        return null;

      // The conversion to handle lack of covariance for Func<out T> in .NET 3.5
      // So that Func<Derived> may be used for Func<Base>
      if (!isAction &&
          serviceExpr.Type != serviceType &&
          !serviceType.GetTypeInfo().IsAssignableFrom(serviceExpr.Type.GetTypeInfo()))
        serviceExpr = Convert(serviceExpr, serviceType);

      return Lambda(wrapperType, serviceExpr, argExprs
#if SUPPORTS_FAST_EXPRESSION_COMPILER
                , isAction ? typeof(void) : serviceType
#endif
                );
    }

    private static Expression GetLambdaExpressionExpressionOrDefault(Request request)
    {
      request = request.Push(request.RequiredServiceType.ThrowIfNull(Error.ResolutionNeedsRequiredServiceType, request));
      var expr = request.Container.ResolveFactory(request)?.GetExpressionOrDefault(request);
      if (expr == null)
        return null;
      return Constant(expr.WrapInFactoryExpression()
#if SUPPORTS_FAST_EXPRESSION_COMPILER
                .ToLambdaExpression()
#endif
                , typeof(System.Linq.Expressions.LambdaExpression));
    }

#if SUPPORTS_FAST_EXPRESSION_COMPILER
    private static Expression GetFastExpressionCompilerLambdaExpressionExpressionOrDefault(Request request)
    {
      request = request.Push(request.RequiredServiceType.ThrowIfNull(Error.ResolutionNeedsRequiredServiceType, request));
      var expr = request.Container.ResolveFactory(request)?.GetExpressionOrDefault(request);
      if (expr == null)
        return null;
      return Constant(expr.WrapInFactoryExpression(), typeof(FastExpressionCompiler.LightExpression.LambdaExpression));
    }
#endif

    private static Expression GetFactoryDelegateExpressionOrDefault(Request request)
    {
      Type serviceType;
      var wrapperType = request.ServiceType;
      if (wrapperType == typeof(FactoryDelegate))
        serviceType = request.RequiredServiceType.ThrowIfNull(Error.ResolutionNeedsRequiredServiceType, request);
      else
        serviceType = request.RequiredServiceType ?? wrapperType.GetGenericParamsAndArgs()[0];

      request = request.Push(serviceType);
      var container = request.Container;
      var expr = container.ResolveFactory(request)?.GetExpressionOrDefault(request);
      if (expr == null)
        return null;

      var rules = container.Rules;
      if (wrapperType == typeof(FactoryDelegate))
        return Constant(expr.CompileToFactoryDelegate(rules.UseFastExpressionCompiler, rules.UseInterpretation));

      return Constant(
          expr.CompileToFactoryDelegate(wrapperType, serviceType, rules.UseFastExpressionCompiler, rules.UseInterpretation),
          wrapperType);
    }

    private static Expression GetKeyValuePairExpressionOrDefault(Request request)
    {
      var keyValueType = request.GetActualServiceType();
      var typeArgs = keyValueType.GetGenericParamsAndArgs();
      var serviceKeyType = typeArgs[0];
      var serviceKey = request.ServiceKey;
      if (serviceKey == null && serviceKeyType.IsValueType() ||
          serviceKey != null && !serviceKeyType.IsTypeOf(serviceKey))
        return null;

      var serviceType = typeArgs[1];
      var serviceRequest = request.Push(serviceType, serviceKey);
      var serviceFactory = request.Container.ResolveFactory(serviceRequest);
      var serviceExpr = serviceFactory?.GetExpressionOrDefault(serviceRequest);
      if (serviceExpr == null)
        return null;

      var keyExpr = request.Container.GetConstantExpression(serviceKey, serviceKeyType);
      return New(
          keyValueType.GetTypeInfo().DeclaredConstructors.First(x => x.GetParameters().Length == 2),
          keyExpr, serviceExpr);
    }

    /// <summary>Discovers and combines service with its setup metadata.
    /// Works with any generic type with first Type arg - Service type and second Type arg - Metadata type,
    /// and constructor with Service and Metadata arguments respectively.
    /// - if service key is not specified in request then method will search for all
    /// registered factories with the same metadata type ignoring keys.
    /// - if metadata is IDictionary{string, object},
    ///  then the First value matching the TMetadata type will be returned.</summary>
    public static Expression GetMetaExpressionOrDefault(Request request)
    {
      var metaType = request.GetActualServiceType();
      var typeArgs = metaType.GetGenericParamsAndArgs();

      var metaCtor = metaType.GetConstructorOrNull(typeArgs)
          .ThrowIfNull(Error.NotFoundMetaCtorWithTwoArgs, typeArgs, request);

      var metadataType = typeArgs[1];
      var serviceType = typeArgs[0];

      var container = request.Container;
      var requiredServiceType = container.GetWrappedType(serviceType, request.RequiredServiceType);

      var factories = container
          .GetAllServiceFactories(requiredServiceType, bothClosedAndOpenGenerics: true) // todo: @perf use the GetServiceRegisteredAndDynamicFactories
          .ToArrayOrSelf();

      if (factories.Length == 0)
        return null;

      var serviceKey = request.ServiceKey;
      if (serviceKey != null)
      {
        factories = factories.Match(serviceKey, (key, f) => key.Equals(f.Key));
        if (factories.Length == 0)
          return null;
      }

      // if the service keys for some reason are not unique
      factories = factories.Match(metadataType, (mType, f) =>
      {
        var metadata = f.Value.Setup.Metadata;
        if (metadata == null)
          return false;

        if (mType == typeof(object))
          return true;

        if (metadata is IDictionary<string, object> metadataDict)
        {
          if (mType == typeof(IDictionary<string, object>))
            return true;
          foreach (var m in metadataDict.Values)
            if (mType.IsTypeOf(m))
              return true;
          return false;
        }

        return mType.IsTypeOf(metadata);
      });

      if (factories.Length == 0)
        return null;

      // Prevent non-determinism when more than 1 factory is matching the metadata
      if (factories.Length > 1)
      {
        if (request.IfUnresolved == IfUnresolved.Throw)
          Throw.It(Error.UnableToSelectFromManyRegistrationsWithMatchingMetadata, metadataType, factories, request);
        return null;
      }

      var factory = factories[0];
      if (factory == null)
        return null;

      serviceKey = factory.Key;

      var serviceRequest = request.Push(serviceType, serviceKey);
      var serviceFactory = container.ResolveFactory(serviceRequest);
      var serviceExpr = serviceFactory?.GetExpressionOrDefault(serviceRequest);
      if (serviceExpr == null)
        return null;

      var resultMetadata = factory.Value.Setup.Metadata;
      if (metadataType != typeof(object))
      {
        var resultMetadataDict = resultMetadata as IDictionary<string, object>;
        if (resultMetadataDict != null && metadataType != typeof(IDictionary<string, object>))
          resultMetadata = resultMetadataDict.Values.FirstOrDefault(oldMap => metadataType.IsTypeOf(oldMap));
      }

      var metadataExpr = container.GetConstantExpression(resultMetadata, metadataType);
      return New(metaCtor, serviceExpr, metadataExpr);
    }
  }

  /// <summary>Represents info required for dynamic registration: service key, factory,
  /// and <see cref="IfAlreadyRegistered"/> option how to combine dynamic with normal registrations.</summary>
  public sealed class DynamicRegistration
  {
    /// <summary>Factory</summary>
    public readonly Factory Factory;

    /// <summary>Optional: will be <see cref="Injector.IfAlreadyRegistered.AppendNotKeyed"/> by default.</summary>
    public readonly IfAlreadyRegistered IfAlreadyRegistered;

    /// <summary>Optional service key: if null the default <see cref="DefaultDynamicKey"/> will be used. </summary>
    public readonly object ServiceKey;

    /// <summary>Constructs the registration</summary>
    public DynamicRegistration(Factory factory,
        IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed, object serviceKey = null)
    {
      Factory = factory.ThrowIfNull().DoNotCache();
      ServiceKey = serviceKey;
      IfAlreadyRegistered = ifAlreadyRegistered;
    }
  }

  /// <summary>The options for the single dynamic registration provider.
  /// The dynamic Wrapper registration is not supported.</summary>
  [Flags]
  public enum DynamicRegistrationFlags : byte
  {
    /// <summary>No flags - to use in `HasDynamicRegistrationProvider`</summary>
    NoFlags = 0,
    /// <summary>Use as AsFallback only</summary>
    AsFallback = 1,
    /// <summary>Provider may have the services provided</summary>
    Service = 1 << 1,
    /// <summary>Provider may have the decorators provided</summary>
    Decorator = 1 << 2,
    /// <summary>Specifies that provider should be asked for the `object` service type to get the decorator for the generic `T` service</summary>
    DecoratorOfAnyTypeViaObjectServiceType = 1 << 3,
  }

  /// <summary> Defines resolution/registration rules associated with Container instance. They may be different for different containers.</summary>
  public sealed class Rules
  {
    /// <summary>Default rules as a staring point.</summary>
    public static readonly Rules Default = new Rules();

    private static Rules WithMicrosoftDependencyInjectionRules(Rules rules)
    {
      rules = rules.Clone(cloneMade: true);
      var settings = rules._settings;
      rules._settings = (settings | Settings.TrackingDisposableTransients)
          & ~Settings.ThrowOnRegisteringDisposableTransient
          & ~Settings.VariantGenericTypesInResolvedCollection;

      rules._factorySelector = SelectLastRegisteredFactory;
      rules._made._factoryMethod = Crystal.FactoryMethod.ConstructorWithResolvableArguments;

      return rules;
    }

    /// <summary>The rules implementing the conventions of Microsoft.Extension.DependencyInjection library.</summary>
    public static readonly Rules MicrosoftDependencyInjectionRules = WithMicrosoftDependencyInjectionRules(Default);

    /// <summary>Returns the copy of the rules with the applied conventions of Microsoft.Extension.DependencyInjection library.</summary>
    public Rules WithMicrosoftDependencyInjectionRules() => WithMicrosoftDependencyInjectionRules(this);

    /// <summary></summary>
    public Rules WithServiceProviderGetServiceShouldThrowIfUnresolved() =>
        WithSettings(_settings | Settings.ServiceProviderGetServiceShouldThrowIfUnresolved);

    /// <summary><see cref="WithServiceProviderGetServiceShouldThrowIfUnresolved"/></summary>
    public bool ServiceProviderGetServiceShouldThrowIfUnresolved =>
        (_settings & Settings.ServiceProviderGetServiceShouldThrowIfUnresolved) != 0;

    /// <summary>Does nothing</summary>
    [Obsolete("Is not used anymore to split the graph - instead use the `DependencyCountInLambdaToSplitBigObjectGraph`")]
    public const int DefaultDependencyDepthToSplitObjectGraph = 20;

    /// <summary>Does nothing</summary>
    [Obsolete("Is not used anymore to split the graph - instead use the `DependencyCountInLambdaToSplitBigObjectGraph`")]
    public int DependencyDepthToSplitObjectGraph { get; private set; }

    /// <summary>The default total dependency count - a expression tree node count to split the object graph</summary>
    public const int DefaultDependencyCountInLambdaToSplitBigObjectGraph = 1024;

    /// <summary>The total dependency count - the expression tree node count to split the object graph.
    /// That does not mean the graph can be always split at this number, consider the example graph and
    /// the dependency count threshold set to 3:
    ///
    /// `x = new X(new Y(A, new B(K), new C(new L(), new M())), new Z())`
    /// 
    /// The tree is resolved from the left to the right in the depth-first order:
    /// A; then K, B (at this point Y is already has 3 dependencies but is not fully resolved until C is resolved);
    /// then L, M, C (here Y is fully resolved with 6 dependencies) so we can split it only on 6 dependencies instead of 3.
    ///
    /// The split itseft just wraps the node in `Func{T}` delegate making it a separate compilation unit.
    /// In our example it will be `Func{Y} f = () => new Y(A, new B(K), new C(new L(), new M()))` considering
    /// that everything is transient.
    /// </summary>
    public int DependencyCountInLambdaToSplitBigObjectGraph { get; private set; }

    /// <summary>Does nothing</summary>
    [Obsolete("It does not work - use `WithDependencyCountInLambdaToSplitBigObjectGraph`")]
    public Rules WithDependencyDepthToSplitObjectGraph(int depth) =>
        new Rules(_settings, FactorySelector, DefaultReuse,
            _made, DefaultIfAlreadyRegistered, depth < 1 ? 1 : depth,
            DependencyResolutionCallExprs, ItemToExpressionConverter,
            DynamicRegistrationProviders, DynamicRegistrationFlags, UnknownServiceResolvers, DefaultRegistrationServiceKey);

    /// <summary>Sets the <see cref="DependencyCountInLambdaToSplitBigObjectGraph"/></summary>
    public Rules WithDependencyCountInLambdaToSplitBigObjectGraph(int dependencyCount) =>
        new Rules(_settings, FactorySelector, DefaultReuse,
            _made, DefaultIfAlreadyRegistered, dependencyCount < 1 ? 1 : dependencyCount,
            DependencyResolutionCallExprs, ItemToExpressionConverter,
            DynamicRegistrationProviders, DynamicRegistrationFlags, UnknownServiceResolvers, DefaultRegistrationServiceKey);

    /// <summary>Does nothing</summary>
    [Obsolete("It does not work - use `WithoutDependencyCountInLambdaToSplitBigObjectGraph`")]
    public Rules WithoutDependencyDepthToSplitObjectGraph() => WithDependencyDepthToSplitObjectGraph(int.MaxValue);

    /// <summary>Disables the <see cref="DependencyCountInLambdaToSplitBigObjectGraph"/> limitation.</summary>
    public Rules WithoutDependencyCountInLambdaToSplitBigObjectGraph() =>
        WithDependencyCountInLambdaToSplitBigObjectGraph(int.MaxValue);

    /// <summary>Shorthand to <see cref="Made.FactoryMethod"/></summary>
    public FactoryMethodSelector FactoryMethod => _made.FactoryMethod;

    /// <summary>Shorthand to <see cref="Made.Parameters"/></summary>
    public ParameterSelector Parameters => _made.Parameters;

    /// <summary>Shorthand to <see cref="Made.PropertiesAndFields"/></summary>
    public PropertiesAndFieldsSelector PropertiesAndFields => _made.PropertiesAndFields;

    /// <summary>Instructs to override per-registration made settings with these rules settings.</summary>
    public bool OverrideRegistrationMade =>
        (_settings & Settings.OverrideRegistrationMade) != 0;

    /// <summary>Returns the parameter selector based on <see cref="OverrideRegistrationMade"/></summary>
    public ParameterSelector TryGetParameterSelector(Made made) =>
        OverrideRegistrationMade ? made.Parameters.OverrideWith(Parameters) : Parameters.OverrideWith(made.Parameters);

    /// <summary>Returns the properties and fields selectorbased on <see cref="OverrideRegistrationMade"/></summary>
    public PropertiesAndFieldsSelector TryGetPropertiesAndFieldsSelector(Made made) =>
        OverrideRegistrationMade
            ? made.PropertiesAndFields.OverrideWith(PropertiesAndFields)
            : PropertiesAndFields.OverrideWith(made.PropertiesAndFields);

    /// <summary>Returns new instance of the rules new Made composed out of
    /// provided factory method, parameters, propertiesAndFields.</summary>
    public Rules With(
        FactoryMethodSelector factoryMethod = null,
        ParameterSelector parameters = null,
        PropertiesAndFieldsSelector propertiesAndFields = null) =>
        With(Made.Of(factoryMethod, parameters, propertiesAndFields));

    /// <summary>Returns new instance of the rules with specified <see cref="Made"/>.</summary>
    /// <param name="made">New Made.Of rules.</param>
    /// <param name="overrideRegistrationMade">Instructs to override registration level Made.Of</param>
    /// <returns>New rules.</returns>
    public Rules With(Made made, bool overrideRegistrationMade = false) =>
        new Rules(
            _settings | (overrideRegistrationMade ? Settings.OverrideRegistrationMade : 0),
            FactorySelector, DefaultReuse,
            _made == Made.Default
                ? made
                : Made.Of(
                    made.FactoryMethod ?? _made.FactoryMethod,
                    made.Parameters ?? _made.Parameters,
                    made.PropertiesAndFields ?? _made.PropertiesAndFields),
            DefaultIfAlreadyRegistered, DependencyCountInLambdaToSplitBigObjectGraph,
            DependencyResolutionCallExprs, ItemToExpressionConverter,
            DynamicRegistrationProviders, DynamicRegistrationFlags, UnknownServiceResolvers, DefaultRegistrationServiceKey);

    /// <summary>Service key to be used instead on `null` in registration.</summary>
    public object DefaultRegistrationServiceKey { get; }

    /// <summary>Sets the <see cref="DefaultRegistrationServiceKey"/></summary>
    public Rules WithDefaultRegistrationServiceKey(object serviceKey) =>
        serviceKey == null ? this :
            new Rules(_settings, FactorySelector, DefaultReuse,
                _made, DefaultIfAlreadyRegistered, DependencyCountInLambdaToSplitBigObjectGraph,
                DependencyResolutionCallExprs, ItemToExpressionConverter,
                DynamicRegistrationProviders, DynamicRegistrationFlags, UnknownServiceResolvers, serviceKey);

    /// <summary>Defines single factory selector delegate.</summary>
    /// <param name="request">Provides service request leading to factory selection.</param>
    /// <param name="factories">Registered factories with corresponding key to select from.</param>
    /// <returns>Single selected factory, or null if unable to select.</returns>
    public delegate Factory FactorySelectorRule(Request request, KeyValuePair<object, Factory>[] factories);

    /// <summary>Rules to select single matched factory default and keyed registered factory/factories.
    /// Selectors applied in specified array order, until first returns not null <see cref="Factory"/>.
    /// Default behavior is to throw on multiple registered default factories, cause it is not obvious what to use.</summary>
    public FactorySelectorRule FactorySelector => _factorySelector;

    /// <summary>Sets <see cref="FactorySelector"/></summary>
    public Rules WithFactorySelector(FactorySelectorRule rule) =>
        new Rules(rule == SelectLastRegisteredFactory ? (_settings | Settings.SelectLastRegisteredFactory) : (_settings & ~Settings.SelectLastRegisteredFactory),
            rule, DefaultReuse, _made, DefaultIfAlreadyRegistered, DependencyCountInLambdaToSplitBigObjectGraph,
            DependencyResolutionCallExprs, ItemToExpressionConverter,
            DynamicRegistrationProviders, DynamicRegistrationFlags, UnknownServiceResolvers, DefaultRegistrationServiceKey);

    /// <summary>Select last registered factory from the multiple default.</summary>
    public static FactorySelectorRule SelectLastRegisteredFactory() => SelectLastRegisteredFactory;
    private static Factory SelectLastRegisteredFactory(Request request, KeyValuePair<object, Factory>[] factories)
    {
      var serviceKey = request.ServiceKey;
      for (var i = factories.Length - 1; i >= 0; --i)
      {
        var factory = factories[i];
        if (factory.Key.Equals(serviceKey))
          return factory.Value;
      }
      return null;
    }

    /// <summary>Tries to select a single factory based on the minimal reuse life-span ignoring the Transients</summary>
    public static FactorySelectorRule SelectFactoryWithTheMinReuseLifespan() => SelectFactoryWithTheMinReuseLifespan;

    /// <summary>Tries either SelectFactoryWithTheMinReuseLifespan or SelectLastRegisteredFactory</summary>
    public static FactorySelectorRule SelectFactoryWithTheMinReuseLifespanOrLastRegistered() => (request, factories) =>
        SelectFactoryWithTheMinReuseLifespan(request, factories) ??
        SelectLastRegisteredFactory(request, factories);

    private static Factory SelectFactoryWithTheMinReuseLifespan(Request request, KeyValuePair<object, Factory>[] factories)
    {
      var minLifespan = int.MaxValue;
      var multipleFactories = false;
      Factory minLifespanFactory = null;

      foreach (var factory in factories)
      {
        var reuse = factory.Value.Reuse;
        var lifespan = reuse == null || reuse == Reuse.Transient ? int.MaxValue : reuse.Lifespan;
        if (lifespan == minLifespan)
          multipleFactories = true;
        else if (lifespan < minLifespan)
        {
          minLifespan = lifespan;
          minLifespanFactory = factory.Value;
          multipleFactories = false;
        }
      }

      return !multipleFactories && minLifespanFactory != null ? minLifespanFactory : null;
    }

    /// <summary>Prefer specified service key (if found) over default key.
    /// Help to override default registrations in Open Scope scenarios:
    /// I may register service with key and resolve it as default in current scope.</summary>
    public static FactorySelectorRule SelectKeyedOverDefaultFactory(object serviceKey) =>
        (r, fs) => fs.FindFirst(serviceKey, (key, f) => f.Key.Equals(key)).Value ??
                   fs.FindFirst(f => f.Key.Equals(null)).Value;

    /// <summary>Specify the method signature for returning multiple keyed factories.
    /// This is dynamic analog to the normal Container Registry.</summary>
    /// <param name="serviceType">Requested service type.</param>
    /// <param name="serviceKey">(optional) If <c>null</c> will request all factories of <paramref name="serviceType"/></param>
    /// <returns>Key-Factory pairs.</returns>
    public delegate IEnumerable<DynamicRegistration> DynamicRegistrationProvider(Type serviceType, object serviceKey);

    /// <summary>Providers for resolving multiple not-registered services. Null by default.</summary>
    public DynamicRegistrationProvider[] DynamicRegistrationProviders { get; private set; }

    /// <summary>The flags per dynamic registration provider</summary>
    public DynamicRegistrationFlags[] DynamicRegistrationFlags { get; private set; }

    /// <summary>Only services and no decorators as it will greately affect the performance, 
    /// calling the provider for every resolved service</summary>
    public static readonly DynamicRegistrationFlags DefaultDynamicRegistrationFlags = Crystal.DynamicRegistrationFlags.Service;

    /// <summary>Get the specific providers with the specified flags and without the flags or return `null` if nothing found</summary>
    public bool HasDynamicRegistrationProvider(DynamicRegistrationFlags withFlags,
        DynamicRegistrationFlags withoutFlags = Crystal.DynamicRegistrationFlags.NoFlags)
    {
      var allFlags = DynamicRegistrationFlags;
      if (allFlags == null || allFlags.Length == 0)
        return false;

      for (var i = 0; i < allFlags.Length; ++i)
      {
        var f = allFlags[i];
        if ((f & withFlags) == withFlags && (f & withoutFlags) == 0)
          return true;
      }

      return false;
    }

    /// <summary>Returns the new rules with the passed dynamic registration rule appended.</summary>
    public Rules WithDynamicRegistration(DynamicRegistrationProvider provider, DynamicRegistrationFlags flags)
    {
      var newRules = Clone(cloneMade: false);
      newRules._settings |= Settings.UseDynamicRegistrationsAsFallbackOnly;
      newRules.DynamicRegistrationProviders = DynamicRegistrationProviders.Append(provider);
      newRules.DynamicRegistrationFlags = DynamicRegistrationFlags.Append(flags);
      return newRules;
    }

    /// <summary>Returns the new rules with the passed dynamic registration rules appended.</summary>
    public Rules WithDynamicRegistrations(params DynamicRegistrationProvider[] rules) =>
        WithDynamicRegistrations(DefaultDynamicRegistrationFlags, rules);

    /// <summary>Returns the new rules with the passed dynamic registration rules appended. 
    /// The rules applied only when no normal registrations found!</summary>
    public Rules WithDynamicRegistrationsAsFallback(params DynamicRegistrationProvider[] rules) =>
        WithDynamicRegistrations(DefaultDynamicRegistrationFlags | Crystal.DynamicRegistrationFlags.AsFallback, rules);


    /// <summary>Returns the new rules with the passed dynamic registration rules appended. 
    /// The rules applied only when no normal registrations found!</summary>
    public Rules WithDynamicRegistrationsAsFallback(DynamicRegistrationFlags flags, params DynamicRegistrationProvider[] rules) =>
        WithDynamicRegistrations(flags | Crystal.DynamicRegistrationFlags.AsFallback, rules);

    /// <summary>Returns the new rules with the passed dynamic registration rules appended. 
    /// The rules applied only when no normal registrations found!</summary>
    public Rules WithDynamicRegistrations(DynamicRegistrationFlags flags, params DynamicRegistrationProvider[] rules)
    {
      var newRules = Clone(cloneMade: false);
      newRules._settings |= Settings.UseDynamicRegistrationsAsFallbackOnly;
      newRules.DynamicRegistrationProviders = DynamicRegistrationProviders.Append(rules);
      newRules.DynamicRegistrationFlags = WithDynamicRegistrationProviderFlags(rules?.Length ?? 0, flags);
      return newRules;
    }

    private DynamicRegistrationFlags[] WithDynamicRegistrationProviderFlags(int count, DynamicRegistrationFlags flags)
    {
      if (count == 0)
        return DynamicRegistrationFlags;

      if (count == 1)
        return DynamicRegistrationFlags.Append(flags);

      var newFlags = new DynamicRegistrationFlags[count];
      for (var i = 0; i < newFlags.Length; i++)
        newFlags[i] = flags;
      return DynamicRegistrationFlags.Append(newFlags);
    }

    // [Obsolete("Instead use `HasDynamicRegistrationProvider`")]
    /// <summary>Obsolete: Instead use `HasDynamicRegistrationProvider`</summary>
    public bool UseDynamicRegistrationsAsFallbackOnly =>
        (_settings & Settings.UseDynamicRegistrationsAsFallbackOnly) != 0;

    /// <summary>Defines delegate to return factory for request not resolved by registered factories or prior rules.
    /// Applied in specified array order until return not null <see cref="Factory"/>.</summary>
    public delegate Factory UnknownServiceResolver(Request request);

    /// <summary>Gets rules for resolving not-registered services. Null by default.</summary>
    public UnknownServiceResolver[] UnknownServiceResolvers { get; private set; }

    /// <summary>Appends resolver to current unknown service resolvers.</summary>
    public Rules WithUnknownServiceResolvers(params UnknownServiceResolver[] rules) =>
        new Rules(_settings, FactorySelector, DefaultReuse,
            _made, DefaultIfAlreadyRegistered, DependencyCountInLambdaToSplitBigObjectGraph, DependencyResolutionCallExprs,
            ItemToExpressionConverter, DynamicRegistrationProviders, DynamicRegistrationFlags,
            UnknownServiceResolvers.Append(rules), DefaultRegistrationServiceKey);

    /// <summary>Removes specified resolver from unknown service resolvers, and returns new Rules.
    /// If no resolver was found then <see cref="UnknownServiceResolvers"/> will stay the same instance,
    /// so it could be check for remove success or fail.</summary>
    public Rules WithoutUnknownServiceResolver(UnknownServiceResolver rule) =>
        new Rules(_settings, FactorySelector, DefaultReuse,
            _made, DefaultIfAlreadyRegistered, DependencyCountInLambdaToSplitBigObjectGraph, DependencyResolutionCallExprs,
            ItemToExpressionConverter, DynamicRegistrationProviders, DynamicRegistrationFlags,
            UnknownServiceResolvers.Remove(rule), DefaultRegistrationServiceKey);

    /// <summary>Sugar on top of <see cref="WithUnknownServiceResolvers"/> to simplify setting the diagnostic action.
    /// Does not guard you from action throwing an exception. Actually can be used to throw your custom exception
    /// instead of <see cref="ContainerException"/>.</summary>
    public Rules WithUnknownServiceHandler(Action<Request> handler) =>
        WithUnknownServiceResolvers(request =>
        {
          handler(request);
          return null;
        });

    /// <summary>The alternative is ConcreteTypeDynamicRegistrations</summary>
    public static UnknownServiceResolver AutoResolveConcreteTypeRule(Func<Request, bool> condition = null) =>
        request =>
        {
          var concreteType = request.GetActualServiceType();
          if (!concreteType.IsImplementationType() || concreteType.IsPrimitive() ||
                  condition != null && !condition(request))
            return null;

          var openGenericServiceType = concreteType.GetGenericDefinitionOrNull();
          if (openGenericServiceType != null && WrappersSupport.Wrappers.GetValueOrDefault(openGenericServiceType) != null)
            return null;

          var factory = new ReflectionFactory(concreteType,
                  made: Crystal.FactoryMethod.ConstructorWithResolvableArgumentsIncludingNonPublic);

              // to enable fallback to other rules if unresolved try to resolve expression first and return null
              return factory.GetExpressionOrDefault(request.WithIfUnresolved(IfUnresolved.ReturnDefault)) != null ? factory : null;
        };

    /// <summary>Rule to automatically resolves non-registered service type which is: nor interface, nor abstract.
    /// For constructor selection we are using <see cref="Injector.FactoryMethod.ConstructorWithResolvableArguments"/>.
    /// The resolution creates transient services.</summary>
    /// <param name="condition">(optional) Condition for requested service type and key.</param>
    /// <param name="reuse">(optional) Reuse for concrete types.</param>
    /// <returns>New rule.</returns>
    public static DynamicRegistrationProvider ConcreteTypeDynamicRegistrations(
        Func<Type, object, bool> condition = null, IReuse reuse = null) =>
        AutoFallbackDynamicRegistrations((serviceType, serviceKey) =>
        {
          if (serviceType.IsAbstract() ||
                  serviceType.IsOpenGeneric() || // service type in principle should be concrete, so should not be open-generic
                  condition != null && !condition(serviceType, serviceKey))
            return null;

              // exclude concrete service types which are pre-defined DryIoc wrapper types
              var openGenericServiceType = serviceType.GetGenericDefinitionOrNull();
          if (openGenericServiceType != null && WrappersSupport.Wrappers.GetValueOrDefault(openGenericServiceType) != null)
            return null;

          return serviceType.One(); // use concrete service type as implementation type
            },
        implType =>
        {
          ReflectionFactory factory = null;

              // the condition checks that factory is resolvable
              factory = new ReflectionFactory(implType, reuse,
	              Crystal.FactoryMethod.ConstructorWithResolvableArgumentsIncludingNonPublic,
                  Setup.With(condition: req => factory?.GetExpressionOrDefault(req.WithIfUnresolved(IfUnresolved.ReturnDefault)) != null));

          return factory;
        });

    /// <summary>Automatically resolves non-registered service type which is: nor interface, nor abstract.
    /// The resolution creates Transient services.</summary>
    public Rules WithConcreteTypeDynamicRegistrations(
        Func<Type, object, bool> condition = null, IReuse reuse = null) =>
        WithDynamicRegistrationsAsFallback(ConcreteTypeDynamicRegistrations(condition, reuse));

    /// Replaced with `WithConcreteTypeDynamicRegistrations`
    public Rules WithAutoConcreteTypeResolution(Func<Request, bool> condition = null) =>
        new Rules(_settings | Settings.AutoConcreteTypeResolution, FactorySelector, DefaultReuse,
            _made, DefaultIfAlreadyRegistered, DependencyCountInLambdaToSplitBigObjectGraph, DependencyResolutionCallExprs,
            ItemToExpressionConverter, DynamicRegistrationProviders, DynamicRegistrationFlags,
            UnknownServiceResolvers.Append(AutoResolveConcreteTypeRule(condition)), DefaultRegistrationServiceKey);

    /// <summary>Creates dynamic fallback registrations for the requested service type
    /// with provided <paramref name="getImplementationTypes"/>.
    /// Fallback means that the dynamic registrations will be applied Only if no normal registrations
    /// exist for the requested service type, hence the "fallback".</summary>
    /// <param name="getImplementationTypes">Implementation types to select for service.</param>
    /// <param name="factory">(optional) Handler to customize the factory, e.g.
    /// specify reuse or setup. Handler should not return <c>null</c>.</param>
    /// <returns>Registration provider.</returns>
    public static DynamicRegistrationProvider AutoFallbackDynamicRegistrations(
        Func<Type, object, IEnumerable<Type>> getImplementationTypes,
        Func<Type, Factory> factory = null)
    {
      // cache factory for the implementation type! to cut on the number of dynamic lookups
      var factories = Ref.Of(ImHashMap<Type, Factory>.Empty);

      return (serviceType, serviceKey) =>
      {
        if (!serviceType.IsServiceType())
          return Enumerable.Empty<DynamicRegistration>();

        var implementationTypes = getImplementationTypes(serviceType, serviceKey);

        return implementationTypes.Match(
            implType => implType.IsImplementingServiceType(serviceType),
            implType =>
            {
              var implTypeHash = RuntimeHelpers.GetHashCode(implType);
              var implFactory = factories.Value.GetValueOrDefault(implTypeHash, implType);
              if (implFactory == null)
              {
                if (factory == null)
                  factories.Swap(fs => (implFactory = fs.GetValueOrDefault(implTypeHash, implType)) != null ? fs
                              : fs.AddOrUpdate(implTypeHash, implType, implFactory = new ReflectionFactory(implType)));
                else
                  factories.Swap(fs => (implFactory = fs.GetValueOrDefault(implTypeHash, implType)) != null ? fs
                              : fs.AddOrUpdate(implTypeHash, implType, implFactory = factory.Invoke(implType).ThrowIfNull()));
              }

                      // We nullify default keys (usually passed by ResolveMany to resolve the specific factory in order)
                      // so that `CombineRegisteredWithDynamicFactories` may assign the key again.
                      // Given that the implementation types are unchanged then the new keys assignment will be the same the last one,
                      // so that the factory resolution will correctly match the required factory by key.
                      // e.g. bitbucket issue #396
                      var theKey = serviceKey is DefaultDynamicKey ? null : serviceKey;
              return new DynamicRegistration(implFactory, IfAlreadyRegistered.Keep, theKey);
            });
      };
    }

    /// <summary>Obsolete: replaced by <see cref="AutoFallbackDynamicRegistrations"/></summary>
    [Obsolete("Replaced by " + nameof(AutoFallbackDynamicRegistrations), false)]
    public static UnknownServiceResolver AutoRegisterUnknownServiceRule(IEnumerable<Type> implTypes,
        Func<IReuse, Request, IReuse> changeDefaultReuse = null, Func<Request, bool> condition = null) =>
        request =>
        {
          if (condition != null && !condition(request))
            return null;

          var scope = request.Container.CurrentScope;
          var reuse = scope != null ? Reuse.ScopedTo(scope.Name) : Reuse.Singleton;

          if (changeDefaultReuse != null)
            reuse = changeDefaultReuse(reuse, request);

          var requestedServiceType = request.GetActualServiceType();
          request.Container.RegisterMany(implTypes, reuse,
                  serviceTypeCondition: serviceType =>
                      serviceType.IsOpenGeneric() && requestedServiceType.IsClosedGeneric()
                          ? serviceType == requestedServiceType.GetGenericTypeDefinition()
                          : serviceType == requestedServiceType);

          return request.Container.GetServiceFactoryOrDefault(request);
        };

    /// <summary>See <see cref="WithDefaultReuse"/></summary>
    public IReuse DefaultReuse { get; }

    /// <summary>The reuse used in case if reuse is unspecified (null) in Register methods.</summary>
    public Rules WithDefaultReuse(IReuse reuse) =>
        reuse == DefaultReuse ? this :
        new Rules(_settings, FactorySelector, reuse ?? Reuse.Transient,
            _made, DefaultIfAlreadyRegistered, DependencyCountInLambdaToSplitBigObjectGraph, DependencyResolutionCallExprs,
            ItemToExpressionConverter, DynamicRegistrationProviders, DynamicRegistrationFlags,
            UnknownServiceResolvers, DefaultRegistrationServiceKey);

    /// <summary>Replaced by WithDefaultReuse because for some cases InsteadOfTransient does not make sense.</summary>
    [Obsolete("Replaced by WithDefaultReuse because for some cases ..InsteadOfTransient does not make sense.", error: false)]
    public Rules WithDefaultReuseInsteadOfTransient(IReuse reuse) => WithDefaultReuse(reuse);

    /// <summary>Given item object and its type should return item "pure" expression presentation,
    /// without side-effects or external dependencies.
    /// e.g. for string "blah" <code lang="cs"><![CDATA[]]>Expression.Constant("blah", typeof(string))</code>.
    /// If unable to convert should return null.</summary>
    public delegate Expression ItemToExpressionConverterRule(object item, Type itemType);

    /// <summary><see cref="WithItemToExpressionConverter"/>.</summary>
    public ItemToExpressionConverterRule ItemToExpressionConverter { get; private set; }

    /// <summary>Specifies custom rule to convert non-primitive items to their expression representation.
    /// That may be required because DryIoc by default does not support non-primitive service keys and registration metadata.
    /// To enable non-primitive values support DryIoc need a way to recreate them as expression tree.</summary>
    public Rules WithItemToExpressionConverter(ItemToExpressionConverterRule itemToExpressionOrDefault) =>
        new Rules(_settings, FactorySelector, DefaultReuse,
            _made, DefaultIfAlreadyRegistered, DependencyCountInLambdaToSplitBigObjectGraph, DependencyResolutionCallExprs,
            itemToExpressionOrDefault, DynamicRegistrationProviders, DynamicRegistrationFlags,
            UnknownServiceResolvers, DefaultRegistrationServiceKey);

    /// <summary><see cref="WithoutThrowIfDependencyHasShorterReuseLifespan"/>.</summary>
    public bool ThrowIfDependencyHasShorterReuseLifespan =>
        (_settings & Settings.ThrowIfDependencyHasShorterReuseLifespan) != 0;

    /// <summary>Turns off throwing exception when dependency has shorter reuse lifespan than its parent or ancestor.</summary>
    /// <returns>New rules with new setting value.</returns>
    public Rules WithoutThrowIfDependencyHasShorterReuseLifespan() =>
        WithSettings(_settings & ~Settings.ThrowIfDependencyHasShorterReuseLifespan);

    /// <summary><see cref="WithThrowIfScopedOrSingletonHasTransientDependency"/>.</summary>
    public bool ThrowIfScopedOrSingletonHasTransientDependency =>
        (_settings & Settings.ThrowIfScopedOrSingletonHasTransientDependency) != 0;

    /// <summary>Turns On throwing the exception when Singleton or Scope service has a Transient dependency</summary>
    public Rules WithThrowIfScopedOrSingletonHasTransientDependency() =>
        WithSettings(_settings | Settings.ThrowIfScopedOrSingletonHasTransientDependency);

    /// <summary>Turns Off throwing the exception when Singleton or Scope service has a Transient dependency (the default)</summary>
    public Rules WithoutThrowIfScopedOrSingletonHasTransientDependency() =>
        WithSettings(_settings & ~Settings.ThrowIfScopedOrSingletonHasTransientDependency);

    /// <summary><see cref="WithoutThrowOnRegisteringDisposableTransient"/></summary>
    public bool ThrowOnRegisteringDisposableTransient =>
        (_settings & Settings.ThrowOnRegisteringDisposableTransient) != 0;

    /// <summary>Turns Off the rule <see cref="ThrowOnRegisteringDisposableTransient"/>.
    /// Allows to register disposable transient but it is up to you to handle their disposal.
    /// You can use <see cref="WithTrackingDisposableTransients"/> to actually track disposable transient in
    /// container, so that disposal will be handled by container.</summary>
    public Rules WithoutThrowOnRegisteringDisposableTransient() =>
        WithSettings(_settings & ~Settings.ThrowOnRegisteringDisposableTransient);

    /// <summary><see cref="WithTrackingDisposableTransients"/></summary>
    public bool TrackingDisposableTransients =>
        (_settings & Settings.TrackingDisposableTransients) != 0;

    /// <summary>
    /// Turns on the storing of disposable transients in the current scope or in the singleton scope if no scopes are opened.
    /// It is required to be able to Dispose the Transient at specific time when the scope is disposed or where container
    /// with singletons is disposed.
    ///
    /// The storing disposable transients in the singleton scope means that they won't be disposed until
    /// the whole container is disposed. That may pose a problem similar to the "memory leak" because more and more transients
    /// will be created and stored never disposed until whole container is disposed. Therefore you 
    /// need to think if you really need the disposable to be the Transient. Whatever, just be aware of it.
    /// </summary>
    public Rules WithTrackingDisposableTransients() =>
        WithSettings((_settings | Settings.TrackingDisposableTransients) & ~Settings.ThrowOnRegisteringDisposableTransient);

    /// <summary>
    /// The opposite of <see cref="WithTrackingDisposableTransients" /> removing the tracking, 
    /// which maybe helpful e.g. for undoing the rule from the Microsoft.DependencyInjection conforming rules.
    /// </summary>
    public Rules WithoutTrackingDisposableTransients() =>
        WithSettings(_settings & ~Settings.TrackingDisposableTransients);

    /// <summary><see cref="WithoutEagerCachingSingletonForFasterAccess"/>.</summary>
    public bool EagerCachingSingletonForFasterAccess =>
        (_settings & Settings.EagerCachingSingletonForFasterAccess) != 0;

    /// <summary>Turns off optimization: creating singletons during resolution of object graph.</summary>
    public Rules WithoutEagerCachingSingletonForFasterAccess() =>
        WithSettings(_settings & ~Settings.EagerCachingSingletonForFasterAccess);

    /// <summary><see cref="WithExpressionGeneration"/>.</summary>
    public Ref<ImHashMap<Request, System.Linq.Expressions.Expression>> DependencyResolutionCallExprs { get; private set; }

    /// <summary>Indicates that container is used for generation purposes, so it should use less runtime state</summary>
    public bool UsedForExpressionGeneration => (_settings & Settings.UsedForExpressionGeneration) != 0;

    private Settings GetSettingsForExpressionGeneration(bool allowRuntimeState = false) =>
        _settings & ~Settings.EagerCachingSingletonForFasterAccess
                  & ~Settings.ImplicitCheckForReuseMatchingScope
                  & ~Settings.UseInterpretationForTheFirstResolution
                  & ~Settings.UseInterpretation
                  | Settings.UsedForExpressionGeneration
                  | (allowRuntimeState ? 0 : Settings.ThrowIfRuntimeStateRequired);

    /// <summary>Specifies to generate ResolutionCall dependency creation expression and stores the result 
    /// in the-per rules collection.</summary>
    public Rules WithExpressionGeneration(bool allowRuntimeState = false) =>
        new Rules(GetSettingsForExpressionGeneration(allowRuntimeState), FactorySelector, DefaultReuse,
            _made, DefaultIfAlreadyRegistered, DependencyCountInLambdaToSplitBigObjectGraph,
            Ref.Of(ImHashMap<Request, System.Linq.Expressions.Expression>.Empty), ItemToExpressionConverter,
            DynamicRegistrationProviders, DynamicRegistrationFlags, UnknownServiceResolvers, DefaultRegistrationServiceKey);

    /// <summary>Indicates that rules are used for the validation, e.g. the rules created in `Validate` method</summary>
    public bool UsedForValidation => (_settings & Settings.UsedForValidation) != 0;

    private Settings GetSettingsForValidation() =>
        _settings & ~Settings.EagerCachingSingletonForFasterAccess
                  & ~Settings.ImplicitCheckForReuseMatchingScope
                  | Settings.UsedForValidation;

    /// <summary>Specifies to generate ResolutionCall dependency creation expression and stores the result 
    /// in the-per rules collection.</summary>
    public Rules ForValidate() =>
        new Rules(GetSettingsForValidation(),
            FactorySelector, DefaultReuse, _made, DefaultIfAlreadyRegistered, int.MaxValue, null,
            ItemToExpressionConverter, DynamicRegistrationProviders, DynamicRegistrationFlags,
            UnknownServiceResolvers, DefaultRegistrationServiceKey);

    /// <summary><see cref="ImplicitCheckForReuseMatchingScope"/></summary>
    public bool ImplicitCheckForReuseMatchingScope =>
        (_settings & Settings.ImplicitCheckForReuseMatchingScope) != 0;

    /// <summary>Removes implicit Factory <see cref="Setup.Condition"/> for non-transient service.
    /// The Condition filters out factory without matching scope.</summary>
    public Rules WithoutImplicitCheckForReuseMatchingScope() =>
        WithSettings(_settings & ~Settings.ImplicitCheckForReuseMatchingScope);

    /// <summary>Removes runtime optimizations preventing an expression generation.</summary>
    public Rules ForExpressionGeneration(bool allowRuntimeState = false) => WithSettings(GetSettingsForExpressionGeneration());

    /// <summary><see cref="WithResolveIEnumerableAsLazyEnumerable"/>.</summary>
    public bool ResolveIEnumerableAsLazyEnumerable =>
        (_settings & Settings.ResolveIEnumerableAsLazyEnumerable) != 0;

    /// <summary>Specifies to resolve IEnumerable as LazyEnumerable.</summary>
    public Rules WithResolveIEnumerableAsLazyEnumerable() =>
        WithSettings(_settings | Settings.ResolveIEnumerableAsLazyEnumerable);

    /// <summary><see cref="WithVariantGenericTypesInResolvedCollection"/>.</summary>
    public bool VariantGenericTypesInResolvedCollection =>
        (_settings & Settings.VariantGenericTypesInResolvedCollection) != 0;

    /// <summary>Flag instructs to include covariant compatible types into the resolved collection.</summary>
    public Rules WithVariantGenericTypesInResolvedCollection() =>
        WithSettings(_settings | Settings.VariantGenericTypesInResolvedCollection);

    /// <summary>Flag instructs to exclude covariant compatible types into the resolved collection.</summary>
    public Rules WithoutVariantGenericTypesInResolvedCollection() =>
        WithSettings(_settings & ~Settings.VariantGenericTypesInResolvedCollection);

    /// <summary><see cref="WithVariantGenericTypesInResolve"/>.</summary>
    public bool VariantGenericTypesInResolve =>
        (_settings & Settings.VariantGenericTypesInResolve) != 0;

    /// <summary>Flag instructs to include covariant compatible types into the resolved generic.</summary>
    public Rules WithVariantGenericTypesInResolve() =>
        WithSettings(_settings | Settings.VariantGenericTypesInResolve);

    /// <summary>Flag instructs to exclude covariant compatible types into the resolved generic.</summary>
    public Rules WithoutVariantGenericTypesInResolve() =>
        WithSettings(_settings & ~Settings.VariantGenericTypesInResolve);

    /// <summary><see cref="WithDefaultIfAlreadyRegistered"/>.</summary>
    public IfAlreadyRegistered DefaultIfAlreadyRegistered { get; }

    /// <summary>Specifies default setting for container. By default is <see cref="IfAlreadyRegistered.AppendNotKeyed"/>.
    /// Example of use: specify Keep as a container default, then set AppendNonKeyed for explicit collection registrations.</summary>
    public Rules WithDefaultIfAlreadyRegistered(IfAlreadyRegistered rule) =>
        rule == DefaultIfAlreadyRegistered ? this :
        new Rules(_settings, FactorySelector, DefaultReuse,
            _made, rule, DependencyCountInLambdaToSplitBigObjectGraph, DependencyResolutionCallExprs, ItemToExpressionConverter,
            DynamicRegistrationProviders, DynamicRegistrationFlags, UnknownServiceResolvers, DefaultRegistrationServiceKey);

    /// <summary><see cref="WithThrowIfRuntimeStateRequired"/>.</summary>
    public bool ThrowIfRuntimeStateRequired =>
        (_settings & Settings.ThrowIfRuntimeStateRequired) != 0;

    /// <summary>Specifies to throw an exception in attempt to resolve service which require runtime state for resolution.
    /// Runtime state may be introduced by RegisterDelegate, RegisterInstance, or registering with non-primitive service key, or metadata.</summary>
    public Rules WithThrowIfRuntimeStateRequired() =>
        WithSettings(_settings | Settings.ThrowIfRuntimeStateRequired);

    /// <summary><see cref="WithCaptureContainerDisposeStackTrace"/>.</summary>
    public bool CaptureContainerDisposeStackTrace =>
        (_settings & Settings.CaptureContainerDisposeStackTrace) != 0;

    /// <summary>Instructs to capture Dispose stack-trace to include it later into <see cref="Error.ContainerIsDisposed"/>
    /// exception for easy diagnostics.</summary>
    public Rules WithCaptureContainerDisposeStackTrace() =>
        WithSettings(_settings | Settings.CaptureContainerDisposeStackTrace);

    /// <summary>Allows Func with args specify its own reuse (sharing) behavior.</summary>
    public bool IgnoringReuseForFuncWithArgs =>
        (_settings & Settings.IgnoringReuseForFuncWithArgs) != 0;

    /// <summary>Allows Func with args specify its own reuse (sharing) behavior.</summary>
    public Rules WithIgnoringReuseForFuncWithArgs() =>
        WithSettings(_settings | Settings.IgnoringReuseForFuncWithArgs);

    /// <summary>Allows Func of service to be resolved even without registered service.</summary>
    public bool FuncAndLazyWithoutRegistration =>
        (_settings & Settings.FuncAndLazyWithoutRegistration) != 0;

    /// <summary>Allows Func of service to be resolved even without registered service.</summary>
    public Rules WithFuncAndLazyWithoutRegistration() =>
        WithSettings(_settings | Settings.FuncAndLazyWithoutRegistration);

    /// Commands to use FastExpressionCompiler - set by default.
    public bool UseFastExpressionCompiler =>
        (_settings & Settings.UseFastExpressionCompilerIfPlatformSupported) != 0;

    /// Fallbacks to system `Expression.Compile()`
    public Rules WithoutFastExpressionCompiler() =>
        WithSettings(_settings & ~Settings.UseFastExpressionCompilerIfPlatformSupported);

    /// Subject-subject
    public bool UseInterpretationForTheFirstResolution =>
        (_settings & Settings.UseInterpretationForTheFirstResolution) != 0;

    /// Fallbacks to system `Expression.Compile()`
    public Rules WithoutInterpretationForTheFirstResolution() =>
        WithSettings(_settings & ~Settings.UseInterpretationForTheFirstResolution & ~Settings.UseInterpretation);

    /// Subject
    public bool UseInterpretation =>
        (_settings & Settings.UseInterpretation) != 0;

    /// <summary>Uses DryIoc own interpretation mechanism or is falling back to `Compile(preferInterpretation: true)`</summary>
    public Rules WithUseInterpretation() =>
        WithSettings(_settings | Settings.UseInterpretation | Settings.UseInterpretationForTheFirstResolution);

    /// <summary>Uses DryIoc own interpretation mechanism or is falling back to `Compile(preferInterpretation: true)`</summary>
    public Rules WithoutUseInterpretation() =>
        WithSettings(_settings & ~Settings.UseInterpretation);

    /// <summary>If Decorator reuse is not set instructs to use `Decorator.SetupWith(useDecarateeReuse: true)`</summary>
    public bool UseDecorateeReuseForDecorators =>
        (_settings & Settings.UseDecorateeReuseForDecorators) != 0;

    /// <summary>If Decorator reuse is not set instructs to use `Decorator.SetupWith(useDecarateeReuse: true)`</summary>
    public Rules WithUseDecorateeReuseForDecorators() =>
        WithSettings(_settings | Settings.UseDecorateeReuseForDecorators);

    /// Outputs most notable non-default rules
    public override string ToString()
    {
      if (this == Default)
        return "Rules.Default";

      string s = "";
      if (_settings != DEFAULT_SETTINGS)
      {
        var addedSettings = _settings & ~DEFAULT_SETTINGS;
        if (addedSettings != 0)
          s = "Rules with {" + addedSettings + "}";
        var removedSettings = DEFAULT_SETTINGS & ~_settings;
        if (removedSettings != 0)
          s += (s != "" ? " and without {" : "Rules without {") + removedSettings + "}";
      }

      if (DependencyCountInLambdaToSplitBigObjectGraph != DefaultDependencyCountInLambdaToSplitBigObjectGraph)
        s += " with TotalDependencyCountInLambdaToSplitBigObjectGraph=" + DependencyCountInLambdaToSplitBigObjectGraph;

      if (DefaultReuse != null && DefaultReuse != Reuse.Transient)
        s += (s != "" ? NewLine : "Rules ") + " with DefaultReuse=" + DefaultReuse;

      if (FactorySelector != null)
      {
        s += (s != "" ? NewLine : "Rules ") + " with FactorySelector=";
        if (FactorySelector == SelectLastRegisteredFactory)
          s += nameof(Rules.SelectLastRegisteredFactory);
        else
          s += "<custom>";
      }

      if (_made != Made.Default)
        s += (s != "" ? NewLine : "Rules ") + " with Made=" + _made;

      return s;
    }

    #region Implementation

    private Rules()
    {
      _made = Made.Default;
      _settings = DEFAULT_SETTINGS;
      DefaultReuse = Reuse.Transient;
      DefaultIfAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed;
      DependencyCountInLambdaToSplitBigObjectGraph = DefaultDependencyCountInLambdaToSplitBigObjectGraph;
    }

    private Rules(Settings settings,
        FactorySelectorRule factorySelector,
        IReuse defaultReuse,
        Made made,
        IfAlreadyRegistered defaultIfAlreadyRegistered,
        int dependencyCountInLambdaToSplitBigObjectGraph,
        Ref<ImHashMap<Request, System.Linq.Expressions.Expression>> dependencyResolutionCallExprs,
        ItemToExpressionConverterRule itemToExpressionConverter,
        DynamicRegistrationProvider[] dynamicRegistrationProviders, DynamicRegistrationFlags[] dynamicRegistrationProvidersFlags,
        UnknownServiceResolver[] unknownServiceResolvers,
        object defaultRegistrationServiceKey)
    {
      _settings = settings;
      _made = made;
      _factorySelector = factorySelector;
      DefaultReuse = defaultReuse;
      DefaultIfAlreadyRegistered = defaultIfAlreadyRegistered;
      DependencyCountInLambdaToSplitBigObjectGraph = dependencyCountInLambdaToSplitBigObjectGraph;
      DependencyResolutionCallExprs = dependencyResolutionCallExprs;
      ItemToExpressionConverter = itemToExpressionConverter;
      DynamicRegistrationProviders = dynamicRegistrationProviders;
      DynamicRegistrationFlags = dynamicRegistrationProvidersFlags;
      UnknownServiceResolvers = unknownServiceResolvers;
      DefaultRegistrationServiceKey = defaultRegistrationServiceKey;
    }

    private Rules Clone(bool cloneMade) =>
        new Rules(
            _settings, FactorySelector, DefaultReuse,
            cloneMade ? _made.Clone() : _made,
            DefaultIfAlreadyRegistered, DependencyCountInLambdaToSplitBigObjectGraph,
            DependencyResolutionCallExprs, ItemToExpressionConverter, DynamicRegistrationProviders,
            DynamicRegistrationFlags, UnknownServiceResolvers, DefaultRegistrationServiceKey);

    private Rules WithSettings(Settings newSettings)
    {
      var newRules = Clone(false);
      newRules._settings = newSettings;
      return newRules;
    }

    private Made _made;

    [Flags]
    private enum Settings
    {
      Empty = 0,
      ThrowIfDependencyHasShorterReuseLifespan = 1 << 1,
      ThrowOnRegisteringDisposableTransient = 1 << 2,
      TrackingDisposableTransients = 1 << 3,
      ImplicitCheckForReuseMatchingScope = 1 << 4,
      VariantGenericTypesInResolvedCollection = 1 << 5,
      ResolveIEnumerableAsLazyEnumerable = 1 << 6,
      EagerCachingSingletonForFasterAccess = 1 << 7,
      ThrowIfRuntimeStateRequired = 1 << 8,
      CaptureContainerDisposeStackTrace = 1 << 9,
      UseDynamicRegistrationsAsFallbackOnly = 1 << 10, // todo: @obsolete now there are individual flags per provider
      IgnoringReuseForFuncWithArgs = 1 << 11,
      OverrideRegistrationMade = 1 << 12,
      FuncAndLazyWithoutRegistration = 1 << 13,
      AutoConcreteTypeResolution = 1 << 14, // informational flag // todo: @clarify consider for the obsoleting
      SelectLastRegisteredFactory = 1 << 15,// informational flag
      UsedForExpressionGeneration = 1 << 16,
      UseFastExpressionCompilerIfPlatformSupported = 1 << 17,
      UseInterpretationForTheFirstResolution = 1 << 18,
      UseInterpretation = 1 << 19,
      UseDecorateeReuseForDecorators = 1 << 20,
      UsedForValidation = 1 << 21, // informational flag, will appear in exceptions during validation
      ServiceProviderGetServiceShouldThrowIfUnresolved = 1 << 22,
      ThrowIfScopedOrSingletonHasTransientDependency = 1 << 23,
      VariantGenericTypesInResolve = 1 << 24,
    }

    private const Settings DEFAULT_SETTINGS
        = Settings.ThrowIfDependencyHasShorterReuseLifespan
        | Settings.ThrowOnRegisteringDisposableTransient
        | Settings.ImplicitCheckForReuseMatchingScope
        | Settings.VariantGenericTypesInResolvedCollection
        | Settings.EagerCachingSingletonForFasterAccess
        | Settings.UseFastExpressionCompilerIfPlatformSupported
        | Settings.UseInterpretationForTheFirstResolution;

    private Settings _settings;
    private FactorySelectorRule _factorySelector;

    #endregion
  }

  /// <summary>Wraps constructor or factory method optionally with factory instance to create service.</summary>
  public sealed class FactoryMethod
  {
    /// <summary>Constructor or method to use for service creation.</summary>
    public readonly MemberInfo ConstructorOrMethodOrMember;

    /// <summary>Identifies factory service if factory method is instance member.</summary>
    public readonly ServiceInfo FactoryServiceInfo;

    /// Alternatively you may just provide an expression for factory
    public readonly Expression FactoryExpression;

    ///<summary> Contains resolved parameter expressions found when looking for most resolvable constructor</summary> 
    internal readonly Expression[] ResolvedParameterExpressions;

    /// <summary>Wraps method and factory instance.
    /// Where <paramref name="ctorOrMethodOrMember"/> is constructor, static or instance method, property or field.</summary>
    public static FactoryMethod Of(MemberInfo ctorOrMethodOrMember, ServiceInfo factoryInfo = null)
    {
      ctorOrMethodOrMember.ThrowIfNull(Error.PassedCtorOrMemberIsNull);

      if (ctorOrMethodOrMember is ConstructorInfo == false && !ctorOrMethodOrMember.IsStatic())
      {
        if (factoryInfo == null)
          Throw.It(Error.PassedMemberIsNotStaticButInstanceFactoryIsNull, ctorOrMethodOrMember);
      }
      else
      {
        if (factoryInfo != null)
          Throw.It(Error.PassedMemberIsStaticButInstanceFactoryIsNotNull, ctorOrMethodOrMember, factoryInfo);
      }

      return new FactoryMethod(ctorOrMethodOrMember, factoryInfo);
    }

    /// <summary>Wraps method and factory instance.
    /// Where <paramref name="methodOrMember"/> is constructor, static or instance method, property or field.</summary>
    public static FactoryMethod Of(MemberInfo methodOrMember, object factoryInstance)
    {
      factoryInstance.ThrowIfNull();
      methodOrMember.ThrowIfNull(Error.PassedCtorOrMemberIsNull);
      if (methodOrMember.IsStatic())
        Throw.It(Error.PassedMemberIsStaticButInstanceFactoryIsNotNull, methodOrMember, factoryInstance);
      return new FactoryMethod(methodOrMember, Constant(factoryInstance));
    }

    /// <summary>Discovers the static factory method or member by name in <typeparamref name="TFactory"/>.
    /// Should play nice with C# <see langword="nameof"/> operator.</summary>
    public static FactoryMethod Of<TFactory>(string methodOrMemberName) =>
        Of(typeof(TFactory).GetAllMembers().FindFirst(m => m.Name == methodOrMemberName).ThrowIfNull());

    /// <summary>Pretty prints wrapped method.</summary>
    public override string ToString()
    {
      var s = new StringBuilder("{")
          .Print(ConstructorOrMethodOrMember.DeclaringType)
          .Append('.').Append(ConstructorOrMethodOrMember);
      if (FactoryServiceInfo != null)
        s.Append(" of factory service ").Append(FactoryServiceInfo);
      if (FactoryExpression != null)
        s.Append(" of factory expression ").Append(FactoryExpression);
      return s.Append('}').ToString();
    }

    private struct CtorWithParameters
    {
      public ConstructorInfo Ctor;
      public ParameterInfo[] Params;
    }

    private static void OrderByParamsLengthDescendingViaInsertionSort(CtorWithParameters[] items)
    {
      int i, j;
      for (i = 1; i < items.Length; ++i)
      {
        var it = items[i];
        for (j = i;
            j >= 1 &&
            it.Params.Length > items[j - 1].Params.Length;
            --j)
        {
          ref var target = ref items[j];
          var source = items[j - 1];
          target.Ctor = source.Ctor;
          target.Params = source.Params;
        }

        ref var x = ref items[j];
        x.Ctor = it.Ctor;
        x.Params = it.Params;
      }
    }

    /// <summary>Easy way to specify non-public and most resolvable constructor.</summary>
    /// <param name="mostResolvable">(optional) Instructs to select constructor with max number of params which all are resolvable.</param>
    /// <param name="includeNonPublic">(optional) Consider the non-public constructors.</param>
    /// <returns>Constructor or null if not found.</returns>
    public static FactoryMethodSelector Constructor(bool mostResolvable = false, bool includeNonPublic = false) => request =>
    {
      var implType = request.ImplementationType.ThrowIfNull(Error.ImplTypeIsNotSpecifiedForAutoCtorSelection, request);
      // todo: @perf we can inline this because we do double checking on the number of constructors
      var ctors = implType.Constructors(includeNonPublic).ToArrayOrSelf();
      var ctorCount = ctors.Length;
      if (ctorCount == 0)
        return null;

      // if there is only one constructor then use it
      if (ctorCount == 1)
        return new FactoryMethod(ctors[0]);

      // stop here if you need a lookup for most resolvable constructor
      if (!mostResolvable)
        return null;

      var paramSelector = request.Rules.TryGetParameterSelector(request.Made)(request);

      var throwIfCtorNotFound = request.IfUnresolved != IfUnresolved.ReturnDefault;
      if (throwIfCtorNotFound)
        request = request.WithIfUnresolved(IfUnresolved.ReturnDefault);

      var ctorsWithParameters = new CtorWithParameters[ctors.Length];
      if (ctors.Length == 2)
      {
        ref var pos0 = ref ctorsWithParameters[0];
        ref var pos1 = ref ctorsWithParameters[1];

        var ctor0Params = ctors[0].GetParameters();
        var ctor1Params = ctors[1].GetParameters();
        if (ctor1Params.Length > ctor0Params.Length)
        {
          pos0.Ctor = ctors[1];
          pos0.Params = ctor1Params;
          pos1.Ctor = ctors[0];
          pos1.Params = ctor0Params;
        }
        else
        {
          pos0.Ctor = ctors[0];
          pos0.Params = ctor0Params;
          pos1.Ctor = ctors[1];
          pos1.Params = ctor1Params;
        }
      }
      else
      {
        for (var i = 0; i < ctors.Length; i++)
        {
          var x = ctors[i];
          ref var pos = ref ctorsWithParameters[i];
          pos.Ctor = x;
          pos.Params = x.GetParameters();
        }

        OrderByParamsLengthDescendingViaInsertionSort(ctorsWithParameters);
      }

      var mostUsedArgCount = -1;
      ConstructorInfo mostResolvedCtor = null;
      Expression[] mostResolvedExprs = null;
      for (var c = 0; c < ctorsWithParameters.Length; ++c)
      {
        var parameters = ctorsWithParameters[c].Params;
        if (parameters.Length == 0)
        {
          mostResolvedCtor = mostResolvedCtor ?? ctorsWithParameters[c].Ctor;
          break;
        }

        // If the most resolved expressions (constructor) is found and the next one has less parameters, we exit. 
        if (mostResolvedExprs != null && mostResolvedExprs.Length > parameters.Length)
          break;

        // Otherwise for similar parameters count constructor we prefer the one with most used input args / custom values
        // Should count custom values provided via `Resolve(args)`, `Func<args..>`, `Parameter.Of...(_ -> arg)`, `container.Use(arg)`
        var usedInputArgOrUsedOrCustomValueCount = 0;
        var inputArgs = request.InputArgExprs;
        var argsUsedMask = 0;
        var paramExprs = new Expression[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
          var param = parameters[i];
          if (inputArgs != null)
          {
            var inputArgExpr =
                ReflectionFactory.TryGetExpressionFromInputArgs(param.ParameterType, inputArgs,
                    ref argsUsedMask);
            if (inputArgExpr != null)
            {
              ++usedInputArgOrUsedOrCustomValueCount;
              paramExprs[i] = inputArgExpr;
              continue;
            }
          }

          var paramInfo = paramSelector(param) ?? ParameterServiceInfo.Of(param);
          var paramRequest = request.Push(paramInfo);
          var paramDetails = paramInfo.Details;
          var usedOrCustomValExpr =
              ReflectionFactory.TryGetUsedInstanceOrCustomValueExpression(request, paramRequest,
                  paramDetails);
          if (usedOrCustomValExpr != null)
          {
            ++usedInputArgOrUsedOrCustomValueCount;
            paramExprs[i] = usedOrCustomValExpr;
            continue;
          }

          var injectedExpr = request.Container.ResolveFactory(paramRequest)?.GetExpressionOrDefault(paramRequest);
          if (injectedExpr == null ||
              // When param is an empty array / collection, then we may use a default value instead (#581)
              paramDetails.DefaultValue != null &&
              injectedExpr.NodeType == System.Linq.Expressions.ExpressionType.NewArrayInit &&
              ((NewArrayExpression)injectedExpr).Expressions.Count == 0)
          {
            // Check if parameter dependency itself (without propagated parent details)
            // does not allow default, then stop checking the rest of parameters.
            if (paramDetails.IfUnresolved == IfUnresolved.Throw)
            {
              paramExprs = null;
              break;
            }

            injectedExpr = paramDetails.DefaultValue != null
                ? request.Container.GetConstantExpression(paramDetails.DefaultValue)
                : paramRequest.ServiceType.GetDefaultValueExpression();
          }

          paramExprs[i] = injectedExpr;
        }

        if (paramExprs != null && usedInputArgOrUsedOrCustomValueCount > mostUsedArgCount)
        {
          mostUsedArgCount = usedInputArgOrUsedOrCustomValueCount;
          mostResolvedCtor = ctorsWithParameters[c].Ctor;
          mostResolvedExprs = paramExprs;
        }
      }

      if (mostResolvedCtor == null)
        return Throw.For<FactoryMethod>(throwIfCtorNotFound,
            Error.UnableToFindCtorWithAllResolvableArgs, request.InputArgExprs, request);

      return new FactoryMethod(mostResolvedCtor, mostResolvedExprs);
    };

    /// <summary>Easy way to specify default constructor to be used for resolution.</summary>
    public static FactoryMethodSelector DefaultConstructor(bool includeNonPublic = false) => request =>
        request.ImplementationType.ThrowIfNull(Error.ImplTypeIsNotSpecifiedForAutoCtorSelection, request)
            .GetConstructorOrNull(includeNonPublic, Empty<Type>())?.To(ctor => new FactoryMethod(ctor));

    /// Better be named `ConstructorWithMostResolvableArguments`.
    /// Searches for public constructor with most resolvable parameters or throws <see cref="ContainerException"/> if not found.
    /// Works both for resolving service and `Func{TArgs..., TService}`
    public static readonly FactoryMethodSelector ConstructorWithResolvableArguments =
        Constructor(mostResolvable: true);

    /// <summary>Searches for constructor (including non public ones) with most
    /// resolvable parameters or throws <see cref="ContainerException"/> if not found.
    /// Works both for resolving service and Func{TArgs..., TService}</summary>
    public static readonly FactoryMethodSelector ConstructorWithResolvableArgumentsIncludingNonPublic =
        Constructor(mostResolvable: true, includeNonPublic: true);

    /// <summary>Just creates a thingy from the constructor</summary>
    public FactoryMethod(ConstructorInfo constructor) =>
        ConstructorOrMethodOrMember = constructor;

    internal FactoryMethod(MemberInfo constructorOrMethodOrMember, ServiceInfo factoryServiceInfo = null)
    {
      ConstructorOrMethodOrMember = constructorOrMethodOrMember;
      FactoryServiceInfo = factoryServiceInfo;
    }

    internal FactoryMethod(MemberInfo constructorOrMethodOrMember, Expression factoryExpression)
    {
      ConstructorOrMethodOrMember = constructorOrMethodOrMember;
      FactoryExpression = factoryExpression;
    }

    internal FactoryMethod(ConstructorInfo ctor, Expression[] resolvedParameterExpressions)
    {
      ConstructorOrMethodOrMember = ctor;
      ResolvedParameterExpressions = resolvedParameterExpressions;
    }
  }

  /// <summary>Rules how to: <list type="bullet">
  /// <item>Select constructor for creating service with <see cref="FactoryMethod"/>.</item>
  /// <item>Specify how to resolve constructor parameters with <see cref="Parameters"/>.</item>
  /// <item>Specify what properties/fields to resolve and how with <see cref="PropertiesAndFields"/>.</item>
  /// </list></summary>
  public class Made
  {
    /// <summary>Returns delegate to select constructor based on provided request.</summary>
    public FactoryMethodSelector FactoryMethod { get => _factoryMethod; private set => _factoryMethod = value; }
    internal FactoryMethodSelector _factoryMethod;

    /// <summary>Return type of strongly-typed factory method expression.</summary>
    public Type FactoryMethodKnownResultType { get; private set; }

    [Flags]
    private enum MadeDetails
    {
      NoConditionals = 0,
      ImplTypeDependsOnRequest = 1 << 1, // todo: @unclear I am not sure why I am using shift to 1 as first and then shift to 3
      ImplMemberDependsOnRequest = 1 << 3,
      HasCustomDependencyValue = 1 << 4
    }

    private readonly MadeDetails _details;

    /// Has any conditional flags
    public bool IsConditional => _details != MadeDetails.NoConditionals;

    /// True is made has properties or parameters with custom value.
    /// That's mean the whole made become context based which affects caching.
    public bool HasCustomDependencyValue => (_details & MadeDetails.HasCustomDependencyValue) != 0;

    /// <summary>Indicates that the implementation type depends on request.</summary>
    public bool IsConditionalImplementation => (_details & MadeDetails.ImplTypeDependsOnRequest) != 0;

    /// Indicates that the member depends on request
    public bool IsImplMemberDependsOnRequest => (_details & MadeDetails.ImplMemberDependsOnRequest) != 0;

    /// <summary>Specifies how constructor parameters should be resolved:
    /// parameter service key and type, throw or return default value if parameter is unresolved.</summary>
    public ParameterSelector Parameters { get; private set; }

    /// <summary>Specifies what <see cref="ServiceInfo"/> should be used when resolving property or field.</summary>
    public PropertiesAndFieldsSelector PropertiesAndFields { get; private set; }

    /// <summary>Outputs whatever is possible (known) for Made</summary>
    public override string ToString()
    {
      if (this == Default)
        return "Made.Default";

      var s = "{";
      if (FactoryMethod != null)
      {
        s += (s == "{" ? "" : ", ") + "FactoryMethod=";
        if (FactoryMethod == Crystal.FactoryMethod.ConstructorWithResolvableArguments)
          s += nameof(Crystal.FactoryMethod.ConstructorWithResolvableArguments);
        else if (FactoryMethod == Crystal.FactoryMethod.ConstructorWithResolvableArgumentsIncludingNonPublic)
          s += nameof(Crystal.FactoryMethod.ConstructorWithResolvableArgumentsIncludingNonPublic);
        else
          s += "<custom>";
      }

      if (FactoryMethodKnownResultType != null)
        s += (s == "{" ? "" : ", ") + "FactoryMethodKnownResultType=" + FactoryMethodKnownResultType;
      if (HasCustomDependencyValue)
        s += (s == "{" ? "" : ", ") + "HasCustomDependencyValue=true";
      if (PropertiesAndFields != null)
        s += (s == "{" ? "" : ", ") + "PropertiesAndFields=<custom>";
      if (Parameters != null)
        s += (s == "{" ? "" : ", ") + "ParameterSelector=<custom>";
      return s + "}";
    }

    /// <summary>Container will use some sensible defaults for service creation.</summary>
    public static readonly Made Default = new Made();

    /// <summary>Creates rules with only <see cref="FactoryMethod"/> specified.</summary>
    public static implicit operator Made(FactoryMethodSelector factoryMethod) => new Made(factoryMethod);

    /// <summary>Creates rules with only <see cref="Parameters"/> specified.</summary>
    public static implicit operator Made(ParameterSelector parameters) => new Made(null, parameters);

    /// <summary>Creates rules with only <see cref="PropertiesAndFields"/> specified.</summary>
    public static implicit operator Made(PropertiesAndFieldsSelector propertiesAndFields) => new Made(null, null, propertiesAndFields);

    // todo: @bug fix the spelling for `isConditionalImlementation`
    /// <summary>Specifies injections rules for Constructor, Parameters, Properties and Fields. If no rules specified returns <see cref="Default"/> rules.</summary>
    public static Made Of(FactoryMethodSelector factoryMethod = null,
        ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null,
        bool isConditionalImlementation = false) =>
        factoryMethod == null && parameters == null && propertiesAndFields == null && !isConditionalImlementation ? Default :
        new Made(factoryMethod, parameters, propertiesAndFields, isConditionalImlementation: isConditionalImlementation);

    /// <summary>Specifies injections rules for Constructor, Parameters, Properties and Fields. If no rules specified returns <see cref="Default"/> rules.</summary>
    /// <param name="factoryMethod">Known factory method.</param>
    /// <param name="parameters">(optional)</param> <param name="propertiesAndFields">(optional)</param>
    /// <returns>New injection rules.</returns>
    public static Made Of(FactoryMethod factoryMethod,
        ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null)
    {
      var methodReturnType = factoryMethod.ThrowIfNull()
          .ConstructorOrMethodOrMember.GetReturnTypeOrDefault();

      // Normalizes open-generic type to open-generic definition,
      // because for base classes and return types it may not be the case (they may be partially closed).
      if (methodReturnType != null && methodReturnType.IsOpenGeneric())
        methodReturnType = methodReturnType.GetGenericTypeDefinition();

      return new Made(factoryMethod.ToFunc<Request, FactoryMethod>, parameters, propertiesAndFields, methodReturnType);
    }

    /// <summary>Creates factory method specification</summary>
    public static Made Of(MemberInfo factoryMethodOrMember, ServiceInfo factoryInfo = null,
        ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null) =>
        Of(Crystal.FactoryMethod.Of(factoryMethodOrMember, factoryInfo), parameters, propertiesAndFields);

    /// <summary>Creates factory specification with implementation type, conditionally depending on request.</summary>
    public static Made Of(Func<Request, Type> getImplType,
        ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null) =>
        Of(r => Crystal.FactoryMethod.Of(getImplType(r).SingleConstructor()),
            parameters, propertiesAndFields, isConditionalImlementation: true);

    /// <summary>Creates factory specification with method or member selector based on request.
    /// Where <paramref name="getMethodOrMember"/> is method, or constructor, or member selector.</summary>
    public static Made Of(Func<Request, MemberInfo> getMethodOrMember, ServiceInfo factoryInfo = null,
        ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null) =>
        new Made(r => Crystal.FactoryMethod.Of(getMethodOrMember(r), factoryInfo),
            parameters, propertiesAndFields, implMemberDependsOnRequest: true);

    /// <summary>Creates factory specification with method or member selector based on request.
    /// Where <paramref name="getMethodOrMember"/>Method, or constructor, or member selector.</summary>
    public static Made Of(Func<Request, MemberInfo> getMethodOrMember, Func<Request, ServiceInfo> factoryInfo,
        ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null) =>
        new Made(r => Crystal.FactoryMethod.Of(getMethodOrMember(r), factoryInfo(r)),
            parameters, propertiesAndFields, implMemberDependsOnRequest: true);

    /// <summary>Defines how to select constructor from implementation type.
    /// Where <paramref name="getConstructor"/> is delegate taking implementation type as input 
    /// and returning selected constructor info.</summary>
    public static Made Of(Func<Type, ConstructorInfo> getConstructor, ParameterSelector parameters = null,
        PropertiesAndFieldsSelector propertiesAndFields = null) =>
        Of(r => Crystal.FactoryMethod.Of(getConstructor(r.ImplementationType).ThrowIfNull(Error.GotNullConstructorFromFactoryMethod, r)),
            parameters, propertiesAndFields);

    /// <summary>Defines factory method using expression of constructor call (with properties), or static method call.</summary>
    /// <typeparam name="TService">Type with constructor or static method.</typeparam>
    /// <param name="serviceReturningExpr">Expression tree with call to constructor with properties:
    /// <code lang="cs"><![CDATA[() => new Car(Arg.Of<IEngine>()) { Color = Arg.Of<Color>("CarColor") }]]></code>
    /// or static method call <code lang="cs"><![CDATA[() => Car.Create(Arg.Of<IEngine>())]]></code></param>
    /// <param name="argValues">(optional) Primitive custom values for dependencies.</param>
    /// <returns>New Made specification.</returns>
    public static TypedMade<TService> Of<TService>(
        System.Linq.Expressions.Expression<Func<TService>> serviceReturningExpr,
        params Func<Request, object>[] argValues) =>
        FromExpression<TService>(member => _ => Crystal.FactoryMethod.Of(member), serviceReturningExpr, argValues);

    /// <summary>Defines creation info from factory method call Expression without using strings.
    /// You can supply any/default arguments to factory method, they won't be used, it is only to find the <see cref="MethodInfo"/>.</summary>
    /// <typeparam name="TFactory">Factory type.</typeparam> <typeparam name="TService">Factory product type.</typeparam>
    /// <param name="getFactoryInfo">Returns or resolves factory instance.</param>
    /// <param name="serviceReturningExpr">Method, property or field expression returning service.</param>
    /// <param name="argValues">(optional) Primitive custom values for dependencies.</param>
    /// <returns>New Made specification.</returns>
    public static TypedMade<TService> Of<TFactory, TService>(
        Func<Request, ServiceInfo.Typed<TFactory>> getFactoryInfo,
        System.Linq.Expressions.Expression<Func<TFactory, TService>> serviceReturningExpr,
        params Func<Request, object>[] argValues)
        where TFactory : class
    {
      getFactoryInfo.ThrowIfNull();
      return FromExpression<TService>(member => request => Crystal.FactoryMethod.Of(member, getFactoryInfo(request)),
          serviceReturningExpr, argValues);
    }

    /// Composes Made.Of expression with known factory instance and expression to get a service
    public static TypedMade<TService> Of<TFactory, TService>(
        TFactory factoryInstance,
        System.Linq.Expressions.Expression<Func<TFactory, TService>> serviceReturningExpr,
        params Func<Request, object>[] argValues)
        where TFactory : class
    {
      factoryInstance.ThrowIfNull();
      return FromExpression<TService>(
          member => request => Crystal.FactoryMethod.Of(member, factoryInstance),
          serviceReturningExpr, argValues);
    }

    private static TypedMade<TService> FromExpression<TService>(
        Func<MemberInfo, FactoryMethodSelector> getFactoryMethodSelector,
        System.Linq.Expressions.LambdaExpression serviceReturningExpr, params Func<Request, object>[] argValues)
    {
      var callExpr = serviceReturningExpr.ThrowIfNull().Body;
      if (callExpr.NodeType == System.Linq.Expressions.ExpressionType.Convert) // proceed without Cast expression.
        return FromExpression<TService>(getFactoryMethodSelector,
            System.Linq.Expressions.Expression.Lambda(((System.Linq.Expressions.UnaryExpression)callExpr).Operand,
                Empty<System.Linq.Expressions.ParameterExpression>()),
            argValues);

      MemberInfo ctorOrMethodOrMember;
      IList<System.Linq.Expressions.Expression> argExprs = null;
      IList<System.Linq.Expressions.MemberBinding> memberBindingExprs = null;
      ParameterInfo[] parameters = null;

      if (callExpr.NodeType == System.Linq.Expressions.ExpressionType.New ||
          callExpr.NodeType == System.Linq.Expressions.ExpressionType.MemberInit)
      {
        var newExpr = callExpr as System.Linq.Expressions.NewExpression ?? ((System.Linq.Expressions.MemberInitExpression)callExpr).NewExpression;
        ctorOrMethodOrMember = newExpr.Constructor;
        parameters = newExpr.Constructor.GetParameters();
        argExprs = newExpr.Arguments;
        if (callExpr is System.Linq.Expressions.MemberInitExpression)
          memberBindingExprs = ((System.Linq.Expressions.MemberInitExpression)callExpr).Bindings;
      }
      else if (callExpr.NodeType == System.Linq.Expressions.ExpressionType.Call)
      {
        var methodCallExpr = (System.Linq.Expressions.MethodCallExpression)callExpr;
        ctorOrMethodOrMember = methodCallExpr.Method;
        parameters = methodCallExpr.Method.GetParameters();
        argExprs = methodCallExpr.Arguments;
      }
      else if (callExpr.NodeType == ExprType.Invoke)
      {
        var invokeExpr = (System.Linq.Expressions.InvocationExpression)callExpr;
        var invokedDelegateExpr = invokeExpr.Expression;
        var invokeMethod = invokedDelegateExpr.Type.GetTypeInfo().GetDeclaredMethod(nameof(Action.Invoke));
        ctorOrMethodOrMember = invokeMethod;
        parameters = invokeMethod.GetParameters();
        argExprs = invokeExpr.Arguments;
      }

      else if (callExpr.NodeType == System.Linq.Expressions.ExpressionType.MemberAccess)
      {
        var member = ((System.Linq.Expressions.MemberExpression)callExpr).Member;
        Throw.If(!(member is PropertyInfo) && !(member is FieldInfo),
            Error.UnexpectedFactoryMemberExpressionInMadeOf, member, serviceReturningExpr);
        ctorOrMethodOrMember = member;
      }
      else return Throw.For<TypedMade<TService>>(Error.NotSupportedMadeOfExpression, callExpr);

      var hasCustomValue = false;

      var parameterSelector = parameters.IsNullOrEmpty() ? null
          : ComposeParameterSelectorFromArgs(ref hasCustomValue,
              serviceReturningExpr, parameters, argExprs, argValues);

      var propertiesAndFieldsSelector =
          memberBindingExprs == null || memberBindingExprs.Count == 0 ? null
          : ComposePropertiesAndFieldsSelector(ref hasCustomValue,
              serviceReturningExpr, memberBindingExprs, argValues);

      return new TypedMade<TService>(getFactoryMethodSelector(ctorOrMethodOrMember),
          parameterSelector, propertiesAndFieldsSelector, hasCustomValue);
    }

    /// <summary>Typed version of <see cref="Made"/> specified with statically typed expression tree.</summary>
    public sealed class TypedMade<TService> : Made
    {
      internal TypedMade(FactoryMethodSelector factoryMethod = null,
          ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null,
          bool hasCustomValue = false)
          : base(factoryMethod, parameters, propertiesAndFields, typeof(TService), hasCustomValue)
      { }
    }

    #region Implementation

    internal Made(
        FactoryMethodSelector factoryMethod = null, ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null,
        Type factoryMethodKnownResultType = null, bool hasCustomValue = false, bool isConditionalImlementation = false,
        bool implMemberDependsOnRequest = false)
    {
      FactoryMethod = factoryMethod;
      Parameters = parameters;
      PropertiesAndFields = propertiesAndFields;
      FactoryMethodKnownResultType = factoryMethodKnownResultType;

      var details = default(MadeDetails);

      if (parameters != null || propertiesAndFields != null)
        details |= MadeDetails.ImplMemberDependsOnRequest;

      if (hasCustomValue)
        details |= MadeDetails.HasCustomDependencyValue;
      if (isConditionalImlementation)
        details |= MadeDetails.ImplTypeDependsOnRequest;
      if (implMemberDependsOnRequest)
        details |= MadeDetails.ImplMemberDependsOnRequest;
      _details = details;
    }

    internal Made(FactoryMethod factoryMethod, Type factoryReturnType)
    {
      FactoryMethod = factoryMethod.ToFunc<Request, FactoryMethod>;
      FactoryMethodKnownResultType = factoryReturnType;
    }

    private Made(
        FactoryMethodSelector factoryMethod, ParameterSelector parameters, PropertiesAndFieldsSelector propertiesAndFields,
        Type factoryMethodKnownResultType, MadeDetails details)
    {
      FactoryMethod = factoryMethod;
      Parameters = parameters;
      PropertiesAndFields = propertiesAndFields;
      FactoryMethodKnownResultType = factoryMethodKnownResultType;
      _details = details;
    }

    internal Made Clone() => new Made(FactoryMethod, Parameters, PropertiesAndFields, FactoryMethodKnownResultType, _details);

    private static ParameterSelector ComposeParameterSelectorFromArgs(ref bool hasCustomValue,
        System.Linq.Expressions.Expression wholeServiceExpr, ParameterInfo[] paramInfos,
        IList<System.Linq.Expressions.Expression> argExprs,
        params Func<Request, object>[] argValues)
    {
      var paramSelector = Crystal.Parameters.Of;
      for (var i = 0; i < argExprs.Count; i++)
      {
        var paramInfo = paramInfos[i];
        var methodCallExpr = argExprs[i] as System.Linq.Expressions.MethodCallExpression;
        if (methodCallExpr != null)
        {
          if (methodCallExpr.Method.DeclaringType != typeof(Arg))
            Throw.It(Error.UnexpectedExpressionInsteadOfArgMethodInMadeOf, methodCallExpr, wholeServiceExpr);

          if (methodCallExpr.Method.Name == Arg.ArgIndexMethodName)
          {
            var getArgValue = GetArgCustomValueProvider(wholeServiceExpr, methodCallExpr, argValues);
            paramSelector = paramSelector.Details((r, p) => p.Equals(paramInfo) ? ServiceDetails.Of(getArgValue(r)) : null);
            hasCustomValue = true;
          }
          else // handle service details
          {
            var defaultValue = paramInfo.IsOptional ? paramInfo.DefaultValue : null;
            var argDetails = GetArgServiceDetails(wholeServiceExpr,
                methodCallExpr, paramInfo.ParameterType, IfUnresolved.Throw, defaultValue);
            paramSelector = paramSelector.Details((r, p) => p.Equals(paramInfo) ? argDetails : null);
          }
        }
        else
        {
          var customValue = GetArgExpressionValueOrThrow(wholeServiceExpr, argExprs[i]);
          paramSelector = paramSelector.Details((r, p) => p.Equals(paramInfo) ? ServiceDetails.Of(customValue) : null);
        }
      }
      return paramSelector;
    }

    private static PropertiesAndFieldsSelector ComposePropertiesAndFieldsSelector(ref bool hasCustomValue,
        System.Linq.Expressions.Expression wholeServiceExpr, IList<System.Linq.Expressions.MemberBinding> memberBindings,
        params Func<Request, object>[] argValues)
    {
      var propertiesAndFields = Crystal.PropertiesAndFields.Of;
      for (var i = 0; i < memberBindings.Count; i++)
      {
        var memberAssignment = (memberBindings[i] as System.Linq.Expressions.MemberAssignment).ThrowIfNull();
        var member = memberAssignment.Member;

        var methodCallExpr = memberAssignment.Expression as System.Linq.Expressions.MethodCallExpression;
        if (methodCallExpr == null) // not an Arg.Of: e.g. constant or variable
        {
          var customValue = GetArgExpressionValueOrThrow(wholeServiceExpr, memberAssignment.Expression);
          propertiesAndFields = propertiesAndFields.OverrideWith(r =>
              PropertyOrFieldServiceInfo.Of(member).WithDetails(ServiceDetails.Of(customValue)).One());
        }
        else
        {
          Throw.If(methodCallExpr.Method.DeclaringType != typeof(Arg),
              Error.UnexpectedExpressionInsteadOfArgMethodInMadeOf, methodCallExpr, wholeServiceExpr);

          if (methodCallExpr.Method.Name == Arg.ArgIndexMethodName) // handle custom value
          {
            var getArgValue = GetArgCustomValueProvider(wholeServiceExpr, methodCallExpr, argValues);
            propertiesAndFields = propertiesAndFields.OverrideWith(req =>
                PropertyOrFieldServiceInfo.Of(member).WithDetails(ServiceDetails.Of(getArgValue(req))).One());
            hasCustomValue = true;
          }
          else
          {
            var memberType = member.GetReturnTypeOrDefault();
            var argServiceDetails = GetArgServiceDetails(wholeServiceExpr, methodCallExpr, memberType, IfUnresolved.ReturnDefault, null);
            propertiesAndFields = propertiesAndFields.OverrideWith(r =>
                PropertyOrFieldServiceInfo.Of(member).WithDetails(argServiceDetails).One());
          }
        }
      }
      return propertiesAndFields;
    }

    private static Func<Request, object> GetArgCustomValueProvider(
        System.Linq.Expressions.Expression wholeServiceExpr,
        System.Linq.Expressions.MethodCallExpression methodCallExpr, Func<Request, object>[] argValues)
    {
      Throw.If(argValues.IsNullOrEmpty(), Error.ArgValueIndexIsProvidedButNoArgValues, wholeServiceExpr);

      var argIndex = (int)GetArgExpressionValueOrThrow(wholeServiceExpr, methodCallExpr.Arguments[0]);
      if (argIndex < 0 || argIndex >= argValues.Length)
        Throw.It(Error.ArgValueIndexIsOutOfProvidedArgValues, argIndex, argValues, wholeServiceExpr);

      return argValues[argIndex];
    }

    private static ServiceDetails GetArgServiceDetails(
        System.Linq.Expressions.Expression wholeServiceExpr,
        System.Linq.Expressions.MethodCallExpression methodCallExpr,
        Type dependencyType, IfUnresolved defaultIfUnresolved, object defaultValue)
    {
      var requiredServiceType = methodCallExpr.Method.GetGenericArguments().Last();
      if (requiredServiceType == dependencyType)
        requiredServiceType = null;

      var serviceKey = default(object);
      var metadataKey = default(string);
      var metadata = default(object);
      var ifUnresolved = defaultIfUnresolved;

      var hasPrevArg = false;

      var argExprs = methodCallExpr.Arguments;
      if (argExprs.Count == 2 &&
          argExprs[0].Type == typeof(string) &&
          argExprs[1].Type != typeof(IfUnresolved)) // matches the Of overload for metadata
      {
        metadataKey = (string)GetArgExpressionValueOrThrow(wholeServiceExpr, argExprs[0]);
        metadata = GetArgExpressionValueOrThrow(wholeServiceExpr, argExprs[1]);
      }
      else
      {
        for (var a = 0; a < argExprs.Count; a++)
        {
          var argValue = GetArgExpressionValueOrThrow(wholeServiceExpr, argExprs[a]);
          if (argValue != null)
          {
            if (argValue is IfUnresolved)
            {
              ifUnresolved = (IfUnresolved)argValue;
              if (hasPrevArg) // the only possible argument is default value.
              {
                defaultValue = serviceKey;
                serviceKey = null;
              }
            }
            else
            {
              serviceKey = argValue;
              hasPrevArg = true;
            }
          }
        }
      }

      return ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, defaultValue, metadataKey, metadata);
    }

    private static object GetArgExpressionValueOrThrow(
        System.Linq.Expressions.Expression wholeServiceExpr,
        System.Linq.Expressions.Expression argExpr)
    {
      var valueExpr = argExpr as System.Linq.Expressions.ConstantExpression;
      if (valueExpr != null)
        return valueExpr.Value;

      var convert = argExpr as System.Linq.Expressions.UnaryExpression; // e.g. (object)SomeEnum.Value
      if (convert != null && convert.NodeType == ExprType.Convert)
        return GetArgExpressionValueOrThrow(wholeServiceExpr,
            convert.Operand as System.Linq.Expressions.ConstantExpression);

      var member = argExpr as System.Linq.Expressions.MemberExpression;
      if (member != null)
      {
        var memberOwner = member.Expression as System.Linq.Expressions.ConstantExpression;
        if (memberOwner != null && memberOwner.Type.IsClosureType() && member.Member is FieldInfo)
          return ((FieldInfo)member.Member).GetValue(memberOwner.Value);
      }

      var newArrExpr = argExpr as System.Linq.Expressions.NewArrayExpression;
      if (newArrExpr != null)
      {
        var itemExprs = newArrExpr.Expressions;
        var items = new object[itemExprs.Count];
        for (var i = 0; i < itemExprs.Count; i++)
          items[i] = GetArgExpressionValueOrThrow(wholeServiceExpr, itemExprs[i]);

        return Converter.ConvertMany(items, newArrExpr.Type.GetElementType());
      }

      return Throw.For<object>(Error.UnexpectedExpressionInsteadOfConstantInMadeOf,
          argExpr, wholeServiceExpr);
    }

    #endregion
  }

  /// <summary>Class for defining parameters/properties/fields service info in <see cref="Made"/> expressions.
  /// Arg methods are NOT actually called, they just used to reflect service info from call expression.</summary>
  public static class Arg
  {
    /// <summary>Specifies required service type of parameter or member. If required type is the same as parameter/member type,
    /// the method is just a placeholder to help detect constructor or factory method, and does not have additional meaning.</summary>
    public static TRequired Of<TRequired>() => default(TRequired);

    /// <summary>Specifies both service and required service types.</summary>
    public static TService Of<TService, TRequired>() => default(TService);

    /// <summary>Specifies required service type of parameter or member. Plus specifies if-unresolved policy.</summary>
    public static TRequired Of<TRequired>(IfUnresolved ifUnresolved) => default(TRequired);

    /// <summary>Specifies both service and required service types.</summary>
    public static TService Of<TService, TRequired>(IfUnresolved ifUnresolved) => default(TService);

    /// <summary>Specifies required service type of parameter or member. Plus specifies service key.</summary>
    public static TRequired Of<TRequired>(object serviceKey) => default(TRequired);

    /// <summary>Specifies both service and required service types.</summary>
    public static TService Of<TService, TRequired>(object serviceKey) => default(TService);

    /// <summary>Specifies required service type of parameter or member. Plus specifies service key.</summary>
    public static TRequired Of<TRequired>(string metadataKey, object metadata) => default(TRequired);

    /// <summary>Specifies both service and required service types.</summary>
    public static TService Of<TService, TRequired>(string metadataKey, object metadata) => default(TService);

    /// <summary>Specifies required service type of parameter or member. Plus specifies if-unresolved policy. Plus specifies service key.</summary>
    public static TRequired Of<TRequired>(IfUnresolved ifUnresolved, object serviceKey) => default(TRequired);

    /// <summary>Specifies both service and required service types.</summary>
    public static TService Of<TService, TRequired>(IfUnresolved ifUnresolved, object serviceKey) => default(TService);

    /// <summary>Specifies required service type, default value and <see cref="IfUnresolved.ReturnDefault"/>.</summary>
    public static TRequired Of<TRequired>(TRequired defaultValue, IfUnresolved ifUnresolved) => default(TRequired);

    /// <summary>Specifies required service type, default value and <see cref="IfUnresolved.ReturnDefault"/>.</summary>
    public static TRequired Of<TRequired>(TRequired defaultValue, IfUnresolved ifUnresolved, object serviceKey) => default(TRequired);

    /// <summary>Specifies argument index starting from 0 to use corresponding custom value factory,
    /// similar to String.Format <c>"{0}, {1}, etc"</c>.</summary>
    public static T Index<T>(int argIndex) => default(T);

    /// <summary>Name is close to method itself to not forget when renaming the method.</summary>
    public static string ArgIndexMethodName = "Index";
  }

  /// <summary>Contains <see cref="IRegistrator"/> extension methods to simplify general use cases.</summary>
  public static class Registrator
  {
    /// <summary>The base method for registering service with its implementation factory. Allows to specify all possible options.</summary>
    public static void Register(this IRegistrator registrator, Type serviceType, Factory factory,
        IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        registrator.Register(factory, serviceType, serviceKey, ifAlreadyRegistered, false);

    /// <summary>Registers service <paramref name="serviceType"/> with corresponding <paramref name="implementationType"/>.</summary>
    public static void Register(this IRegistrator registrator, Type serviceType, Type implementationType,
        IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
        object serviceKey = null) =>
        registrator.Register(new ReflectionFactory(implementationType, reuse, made, setup),
            serviceType, serviceKey, ifAlreadyRegistered, false);

    /// <summary>Registers service of <paramref name="serviceAndMayBeImplementationType"/>.
    /// ServiceType may be the same as <paramref name="serviceAndMayBeImplementationType"/>.</summary>
    public static void Register(this IRegistrator registrator, Type serviceAndMayBeImplementationType,
        IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
        object serviceKey = null) =>
        registrator.Register(new ReflectionFactory(serviceAndMayBeImplementationType, reuse, made, setup),
            serviceAndMayBeImplementationType, serviceKey, ifAlreadyRegistered, false);

    /// <summary>Registers service of <typeparamref name="TService"/> type
    /// implemented by <typeparamref name="TImplementation"/> type.</summary>
    public static void Register<TService, TImplementation>(this IRegistrator registrator,
        IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
        object serviceKey = null)
        where TImplementation : TService =>
        registrator.Register(new ReflectionFactory(typeof(TImplementation), reuse, made, setup),
            typeof(TService), serviceKey, ifAlreadyRegistered, isStaticallyChecked: true);

    /// <summary>Registers implementation type <typeparamref name="TImplementation"/> with itself as service type.</summary>
    public static void Register<TImplementation>(this IRegistrator registrator,
        IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
        object serviceKey = null) =>
        registrator.Register<TImplementation, TImplementation>(reuse, made, setup, ifAlreadyRegistered, serviceKey);

    /// <summary>Registers service type returned by Made expression.</summary>
    public static void Register<TService, TMadeResult>(this IRegistrator registrator,
        Made.TypedMade<TMadeResult> made, IReuse reuse = null, Setup setup = null,
        IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) where TMadeResult : TService =>
        registrator.Register(new ReflectionFactory(default(Type), reuse, made, setup),
            typeof(TService), serviceKey, ifAlreadyRegistered, isStaticallyChecked: true);

    /// <summary>Registers service returned by Made expression.</summary>
    public static void Register<TService>(this IRegistrator registrator,
        Made.TypedMade<TService> made, IReuse reuse = null, Setup setup = null,
        IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        registrator.Register<TService, TService>(made, reuse, setup, ifAlreadyRegistered, serviceKey);

    /// <summary>
    /// Registers the instance creating a "normal" DryIoc registration so you can check it via `IsRegestered`, 
    /// apply wrappers and decorators, etc.
    /// Additionally, if instance is `IDisposable`, then it tracks it in a singleton scope.
    /// NOTE: Look at the `Use` method to put instance directly into current or singleton scope,
    /// though without ability to use decorators and wrappers on it.
    /// </summary>
    public static void RegisterInstance(this IRegistrator registrator, bool isChecked, Type serviceType, object instance,
        IfAlreadyRegistered? ifAlreadyRegistered = null, Setup setup = null, object serviceKey = null)
    {
      registrator.Register(new RegisteredInstanceFactory(instance, Reuse.Singleton, setup),
          serviceType, serviceKey, ifAlreadyRegistered, isStaticallyChecked: false);

      // done after registration to pass all the registration validation checks
      if (instance is IDisposable d && (setup == null || (!setup.PreventDisposal && !setup.WeaklyReferenced)))
        (registrator as IResolverContext)?.SingletonScope.TrackDisposableWithoutDisposalOrder(d);
    }

    /// <summary>
    /// Registers the instance creating a "normal" DryIoc registration so you can check it via `IsRegestered`, 
    /// apply wrappers and decorators, etc.
    /// Additionally, if instance is `IDisposable`, then it tracks it in a singleton scope.
    /// NOTE: Look at the `Use` method to put instance directly into current or singleton scope,
    /// though without ability to use decorators and wrappers on it.
    /// </summary>
    public static void RegisterInstance(this IRegistrator registrator, Type serviceType, object instance,
        IfAlreadyRegistered? ifAlreadyRegistered = null, Setup setup = null, object serviceKey = null) =>
        registrator.RegisterInstance(false, serviceType, instance, ifAlreadyRegistered, setup, serviceKey);

    /// <summary>
    /// Registers the instance creating a "normal" DryIoc registration so you can check it via `IsRegestered`, 
    /// apply wrappers and decorators, etc.
    /// Additionally, if instance is `IDisposable`, then it tracks it in a singleton scope.
    /// NOTE: Look at the `Use` method to put instance directly into current or singleton scope,
    /// though without ability to use decorators and wrappers on it.
    /// </summary>
    public static void RegisterInstance<T>(this IRegistrator registrator, T instance,
        IfAlreadyRegistered? ifAlreadyRegistered = null, Setup setup = null, object serviceKey = null) =>
        registrator.RegisterInstance(true, typeof(T), instance, ifAlreadyRegistered, setup, serviceKey);

    // todo: @feature option to switch off NoServicesWereRegisteredByRegisterMany
    /// <summary>
    /// Registers the instance with possible multiple service types creating a "normal" DryIoc registration 
    /// so you can check it via `IsRegestered` for each service type, 
    /// apply wrappers and decorators, etc.
    /// Additionally, if instance is `IDisposable`, then it tracks it in a singleton scope.
    /// NOTE: Look at the `Use` method to put instance directly into current or singleton scope,
    /// though without ability to use decorators and wrappers on it.
    /// </summary>
    public static void RegisterInstanceMany(this IRegistrator registrator, Type implType, object instance,
        bool nonPublicServiceTypes = false,
        IfAlreadyRegistered? ifAlreadyRegistered = null, Setup setup = null, object serviceKey = null)
    {
      instance.ThrowIfNull();
      if (implType != null)
        instance.ThrowIfNotInstanceOf(implType);
      else
        implType = instance.GetType();

      var serviceTypes = implType.GetImplementedServiceTypes(nonPublicServiceTypes);

      if (serviceTypes.Length == 0)
        Throw.It(Error.NoServicesWereRegisteredByRegisterMany, implType.One());

      var factory = new RegisteredInstanceFactory(instance, Reuse.Singleton, setup);
      foreach (var serviceType in serviceTypes)
        registrator.Register(factory, serviceType, serviceKey, ifAlreadyRegistered, isStaticallyChecked: true);

      if (instance is IDisposable d &&
          (setup == null || (!setup.PreventDisposal && !setup.WeaklyReferenced)))
        (registrator as IResolverContext)?.SingletonScope.TrackDisposableWithoutDisposalOrder(d);
    }

    /// <summary>
    /// Registers the instance with possible multiple service types creating a "normal" DryIoc registration 
    /// so you can check it via `IsRegestered` for each service type, 
    /// apply wrappers and decorators, etc.
    /// Additionally, if instance is `IDisposable`, then it tracks it in a singleton scope.
    /// NOTE: Look at the `Use` method to put instance directly into current or singleton scope,
    /// though without ability to use decorators and wrappers on it.
    /// </summary>
    public static void RegisterInstanceMany<T>(this IRegistrator registrator, T instance,
        bool nonPublicServiceTypes = false,
        IfAlreadyRegistered? ifAlreadyRegistered = null, Setup setup = null, object serviceKey = null) =>
        registrator.RegisterInstanceMany(instance.GetType(), instance,
            nonPublicServiceTypes, ifAlreadyRegistered, setup, serviceKey);

    /// <summary>
    /// Registers the instance with possible multiple service types creating a "normal" DryIoc registration 
    /// so you can check it via `IsRegestered` for each service type, 
    /// apply wrappers and decorators, etc.
    /// Additionally, if instance is `IDisposable`, then it tracks it in a singleton scope.
    /// NOTE: Look at the `Use` method to put instance directly into current or singleton scope,
    /// though without ability to use decorators and wrappers on it.
    /// </summary>
    public static void RegisterInstanceMany(this IRegistrator registrator, Type[] serviceTypes, object instance,
        IfAlreadyRegistered? ifAlreadyRegistered = null, Setup setup = null, object serviceKey = null)
    {
      var instanceType = instance.GetType();
      if (serviceTypes.IsNullOrEmpty())
        Throw.It(Error.NoServicesWereRegisteredByRegisterMany, instance);

      var factory = new RegisteredInstanceFactory(instance, Reuse.Singleton, setup);

      foreach (var serviceType in serviceTypes)
      {
        serviceType.ThrowIfNotImplementedBy(instanceType);
        registrator.Register(factory, serviceType, serviceKey, ifAlreadyRegistered, isStaticallyChecked: true);
      }

      if (instance is IDisposable d &&
          (setup == null || (!setup.PreventDisposal && !setup.WeaklyReferenced)))
        (registrator as IResolverContext)?.SingletonScope.TrackDisposableWithoutDisposalOrder(d);
    }

    /// <summary>Checks some common .NET types to exclude.</summary>
    public static bool IsExcludedGeneralPurposeServiceType(this Type type)
    {
      if (type == typeof(object))
        return true;
      if (type == typeof(string))
        return true;
      if (type == typeof(IDisposable))
        return true;
      if (type == typeof(IComparable))
        return true;
#if SUPPORTS_SERIALIZABLE
      if (type == typeof(System.Runtime.Serialization.ISerializable))
        return true;
#endif
#if SUPPORTS_ICLONEABLE
      if (type == typeof(ICloneable))
        return true;
#endif
      if (type.IsGeneric())
      {
        var genType = type.GetGenericTypeDefinition();
        if (genType == typeof(IEquatable<>))
          return true;
      }
      return false;
    }

    /// <summary>Checks that type can be used a service type.</summary>
    public static bool IsServiceType(this Type type) =>
        !type.IsPrimitive() && !type.IsExcludedGeneralPurposeServiceType() && !type.IsCompilerGenerated();

    /// <summary>Checks if type can be used as implementation type for reflection factory,
    /// and therefore registered to container. Usually used to discover implementation types from assembly.</summary>
    public static bool IsImplementationType(this Type type) =>
        type.IsClass() && !type.IsAbstract() && !type.IsCompilerGenerated();

    /// <summary>Returns only those types that could be used as service types of <paramref name="type"/>.
    /// It means that for open-generic <paramref name="type"/> its service type should supply all type arguments.</summary>
    public static Type[] GetImplementedServiceTypes(this Type type, bool nonPublicServiceTypes = false)
    {
      var implementedTypes = type.GetImplementedTypes(ReflectionTools.AsImplementedType.SourceType);

      var serviceTypes = nonPublicServiceTypes
          ? implementedTypes.Match(t => t.IsServiceType())
          : implementedTypes.Match(t => t.IsPublicOrNestedPublic() && t.IsServiceType());

      if (type.IsGenericDefinition())
        serviceTypes = serviceTypes.Match(type.GetGenericParamsAndArgs(),
            (paramsAndArgs, x) => x.ContainsAllGenericTypeParameters(paramsAndArgs),
            (_, x) => x.GetGenericDefinitionOrNull());

      return serviceTypes;
    }

    // todo: @bug @perf why don't we just IsAssignableFrom
    /// <summary>The same `GetImplementedServiceTypes` but instead of collecting the service types just check the <paramref name="serviceType"/> is implemented</summary>
    public static bool IsImplementingServiceType(this Type type, Type serviceType)
    {
      if (serviceType == type || serviceType == typeof(object))
        return true;

      var implTypeInfo = type.GetTypeInfo();
      if (!implTypeInfo.IsGenericTypeDefinition)
      {
        if (serviceType.IsInterface())
        {
          foreach (var iface in implTypeInfo.ImplementedInterfaces)
            if (iface == serviceType)
              return true;
        }
        else
        {
          var baseType = implTypeInfo.BaseType;
          for (; baseType != null && baseType != typeof(object); baseType = baseType.GetTypeInfo().BaseType)
            if (serviceType == baseType)
              return true;
        }
      }
      else if (serviceType.IsGenericDefinition())
      {
        var implTypeParams = implTypeInfo.GenericTypeParameters;
        if (serviceType.IsInterface())
        {
          foreach (var iface in implTypeInfo.ImplementedInterfaces)
            if (iface.GetGenericDefinitionOrNull() == serviceType &&
                iface.ContainsAllGenericTypeParameters(implTypeParams))
              return true;
        }
        else
        {
          var baseType = implTypeInfo.BaseType;
          for (; baseType != null && baseType != typeof(object); baseType = baseType.GetTypeInfo().BaseType)
            if (baseType.GetGenericDefinitionOrNull() == serviceType &&
                baseType.ContainsAllGenericTypeParameters(implTypeParams))
              return true;
        }
      }

      return false;
    }

    /// <summary>Returns the sensible services automatically discovered for RegisterMany implementation type.
    /// Excludes the collection wrapper interfaces. The <paramref name="type"/> may be concrete, abstract or
    /// generic definition.</summary>
    public static Type[] GetRegisterManyImplementedServiceTypes(this Type type, bool nonPublicServiceTypes = false) =>
        GetImplementedServiceTypes(type, nonPublicServiceTypes)
            .Match(t => !t.IsGenericDefinition() || WrappersSupport.SupportedCollectionTypes.IndexOfReference(t) == -1);

    /// <summary>Returns the types suitable to be an implementation types for <see cref="ReflectionFactory"/>:
    /// actually a non abstract and not compiler generated classes.</summary>
    public static IEnumerable<Type> GetImplementationTypes(this Assembly assembly) =>
        Portable.GetAssemblyTypes(assembly).Where(IsImplementationType);

    /// <summary>Returns the types suitable to be an implementation types for <see cref="ReflectionFactory"/>:
    /// actually a non abstract and not compiler generated classes.</summary>
    public static IEnumerable<Type> GetImplementationTypes(this Assembly assembly, Func<Type, bool> condition) =>
        Portable.GetAssemblyTypes(assembly).Where(t => condition(t) && t.IsImplementationType());

    /// <summary>Sugar, so you can say <code lang="cs"><![CDATA[r.RegisterMany<X>(Registrator.Interfaces)]]></code></summary>
    public static Func<Type, bool> Interfaces = ReflectionTools.IsInterface;

    /// <summary>Checks if <paramref name="type"/> implements a service type,
    /// along the checking if <paramref name="type"/> is a valid implementation type.</summary>
    public static bool ImplementsServiceType(this Type type, Type serviceType) =>
        type.IsImplementationType() && type.IsImplementingServiceType(serviceType);

    /// <summary>Checks if <paramref name="type"/> implements a service type,
    /// along the checking if <paramref name="type"/> and service type
    /// are valid implementation and service types.</summary>
    public static bool ImplementsServiceType<TService>(this Type type) =>
        type.ImplementsServiceType(typeof(TService));

    /// <summary>Wraps the implementation type in factory.</summary>
    public static Factory ToFactory(this Type implType) => new ReflectionFactory(implType);

    /// <summary>Wraps the implementation type in factory plus allow to provide factory parameters.</summary>
    public static Factory ToFactory(this Type implType, IReuse reuse = null, Made made = null, Setup setup = null) =>
        new ReflectionFactory(implType, reuse, made, setup);

    /// <summary>
    /// Batch registering the implementations with possibly many service types,
    /// throwing the <see cref="Error.NoServicesWereRegisteredByRegisterMany" /> error when there are no services types to register.
    /// You may pass the predefined <see cref="GetRegisterManyImplementedServiceTypes"/> to <paramref name="getServiceTypes"/>.
    /// By default <paramref name="getImplFactory"/> uses the <see cref="ReflectionFactory"/> with the default reuse,
    /// or you may return the <see cref="ReflectionFactory"/> with the <see cref="Reuse"/> of your choice.
    /// </summary>
    public static void RegisterMany(this IRegistrator registrator,
        IEnumerable<Type> implTypes,
        Func<Type, Type[]> getServiceTypes,
        Func<Type, Factory> getImplFactory = null,
        Func<Type, Type, object> getServiceKey = null,
        IfAlreadyRegistered? ifAlreadyRegistered = null)
    {
      getImplFactory = getImplFactory ?? ToFactory;

      var isSomethingRegistered = false;
      var anyImplTypes = false;
      foreach (var implType in implTypes)
      {
        anyImplTypes = true;
        var serviceTypes = getServiceTypes(implType);
        if (!serviceTypes.IsNullOrEmpty())
        {
          var factory = getImplFactory(implType);
          for (var i = 0; i < serviceTypes.Length; i++)
          {
            var t = serviceTypes[i];
            registrator.Register(t, factory, ifAlreadyRegistered, getServiceKey?.Invoke(implType, t));
            isSomethingRegistered = true;
          }
        }
      }

      if (anyImplTypes && !isSomethingRegistered)
        Throw.It(Error.NoServicesWereRegisteredByRegisterMany, implTypes);
    }

    /// <summary>
    /// Batch registering the implementations with possibly many service types,
    /// ignoring the case when there are no services types to register.
    /// You may pass the predefined <see cref="GetRegisterManyImplementedServiceTypes"/> to <paramref name="getServiceTypes"/>.
    /// By default <paramref name="getImplFactory"/> uses the <see cref="ReflectionFactory"/> with the default reuse,
    /// or you may return the <see cref="ReflectionFactory"/> with the <see cref="Reuse"/> of your choice.
    /// </summary>
    public static void RegisterManyIgnoreNoServicesWereRegistered(this IRegistrator registrator,
        IEnumerable<Type> implTypes,
        Func<Type, Type[]> getServiceTypes,
        Func<Type, Factory> getImplFactory = null,
        Func<Type, Type, object> getServiceKey = null,
        IfAlreadyRegistered? ifAlreadyRegistered = null)
    {
      getImplFactory = getImplFactory ?? ToFactory;

      foreach (var implType in implTypes)
      {
        var serviceTypes = getServiceTypes(implType);
        if (!serviceTypes.IsNullOrEmpty())
        {
          var factory = getImplFactory(implType);
          for (var i = 0; i < serviceTypes.Length; i++)
          {
            var t = serviceTypes[i];
            registrator.Register(t, factory, ifAlreadyRegistered, getServiceKey?.Invoke(implType, t));
          }
        }
      }
    }

    /// <summary>Batch registers implementation with possibly many service types.</summary>
    public static void RegisterMany(this IRegistrator registrator,
        Type[] serviceTypes, Type implType,
        IReuse reuse = null, Made made = null, Setup setup = null,
        IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        registrator.RegisterMany(new[] { implType }, serviceTypes.ToFunc<Type, Type[]>,
            t => t.ToFactory(reuse, made, setup), (_, __) => serviceKey, ifAlreadyRegistered);

    /// <summary>Batch registers assemblies of implementation types with possibly many service types.
    /// The default factory is the <see cref="ReflectionFactory"/> with default reuse.</summary>
    public static void RegisterMany(this IRegistrator registrator,
        IEnumerable<Assembly> implTypeAssemblies, Func<Type, Type[]> getServiceTypes,
        Func<Type, Factory> getImplFactory = null, Func<Type, Type, object> getServiceKey = null,
        IfAlreadyRegistered? ifAlreadyRegistered = null) =>
        registrator.RegisterMany(implTypeAssemblies.ThrowIfNull().SelectMany(GetImplementationTypes),
            getServiceTypes, getImplFactory, getServiceKey, ifAlreadyRegistered);

    /// <summary>Registers many implementations with their auto-figured service types.</summary>
    public static void RegisterMany(this IRegistrator registrator,
        IEnumerable<Assembly> implTypeAssemblies, Func<Type, bool> serviceTypeCondition,
        IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
        bool nonPublicServiceTypes = false, object serviceKey = null) =>
        registrator.RegisterMany(implTypeAssemblies.ThrowIfNull().SelectMany(GetImplementationTypes),
            reuse, made, setup, ifAlreadyRegistered, serviceTypeCondition, nonPublicServiceTypes, serviceKey);

    /// <summary>Registers many implementations with auto-figured service types.</summary>
    public static void RegisterMany(this IRegistrator registrator, IEnumerable<Type> implTypes,
        IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
        Func<Type, bool> serviceTypeCondition = null, bool nonPublicServiceTypes = false,
        object serviceKey = null) =>
        registrator.RegisterMany(implTypes,
            t => t.GetRegisterManyImplementedServiceTypes(nonPublicServiceTypes).Match(serviceTypeCondition ?? Fun.Always),
            reuse == null && made == null && setup == null ? default(Func<Type, Factory>) : t => t.ToFactory(reuse, made, setup),
            serviceKey == null ? default(Func<Type, Type, object>) : (i, s) => serviceKey,
            ifAlreadyRegistered);

    /// <summary>Registers single registration for all implemented public interfaces and base classes.</summary>
    public static void RegisterMany<TImplementation>(this IRegistrator registrator,
        IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
        Func<Type, bool> serviceTypeCondition = null, bool nonPublicServiceTypes = false,
        object serviceKey = null) =>
        registrator.RegisterMany(typeof(TImplementation).One(),
            reuse, made, setup, ifAlreadyRegistered, serviceTypeCondition, nonPublicServiceTypes, serviceKey);

    /// <summary>Registers single registration for all implemented public interfaces and base classes.</summary>
    public static void RegisterMany<TMadeResult>(this IRegistrator registrator,
        Made.TypedMade<TMadeResult> made,
        IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
        Func<Type, bool> serviceTypeCondition = null, bool nonPublicServiceTypes = false,
        object serviceKey = null) =>
        registrator.RegisterMany<TMadeResult>(reuse, made.ThrowIfNull(), setup,
            ifAlreadyRegistered, serviceTypeCondition, nonPublicServiceTypes, serviceKey);

    /// <summary>Registers a factory delegate for creating an instance of <typeparamref name="TService"/>.
    /// Delegate can use resolver context parameter to resolve any required dependencies, e.g.:
    /// <code lang="cs"><![CDATA[container.RegisterDelegate<ICar>(r => new Car(r.Resolve<IEngine>()))]]></code></summary>
    /// <remarks>The alternative to this method please consider using <see cref="Made"/> instead:
    /// <code lang="cs"><![CDATA[container.Register<ICar>(Made.Of(() => new Car(Arg.Of<IEngine>())))]]></code>.
    /// </remarks>
    public static void RegisterDelegate<TService>(this IRegistrator registrator,
        Func<IResolverContext, TService> factoryDelegate,
        IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
        object serviceKey = null) =>
        registrator.Register(new DelegateFactory(factoryDelegate.ToFactoryDelegate, reuse, setup),
            typeof(TService), serviceKey, ifAlreadyRegistered, isStaticallyChecked: true);

    /// <summary>Registers delegate to be injected by container avoiding the ServiceLocator anti-pattern</summary>
    public static void RegisterDelegate<TService>(
        this IRegistrator r, Func<TService> factory,
        IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        RegisterDelegateFunc(r, typeof(TService), factory, reuse, setup, ifAlreadyRegistered, serviceKey);

    /// <summary>Registers delegate with explicit arguments to be injected by container avoiding the ServiceLocator anti-pattern</summary>
    public static void RegisterDelegate<TDep1, TService>(
        this IRegistrator r, Func<TDep1, TService> factory,
        IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        RegisterDelegateFunc(r, typeof(TService), factory, reuse, setup, ifAlreadyRegistered, serviceKey);

    /// <summary>Registers delegate with explicit arguments to be injected by container avoiding the ServiceLocator anti-pattern</summary>
    public static void RegisterDelegate<TDep1>(
        this IRegistrator r, Type serviceType, Func<TDep1, object> factory,
        IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        RegisterDelegateFunc<Func<TDep1, object>>(r, serviceType,
            dep1 => factory(dep1).ThrowIfNotInstanceOf(serviceType, Error.RegisteredDelegateResultIsNotOfServiceType),
            reuse, setup, ifAlreadyRegistered, serviceKey);

    /// <summary>Registers delegate with explicit arguments to be injected by container avoiding the ServiceLocator anti-pattern</summary>
    public static void RegisterDelegate<TDep1, TDep2, TService>(
        this IRegistrator r, Func<TDep1, TDep2, TService> factory,
        IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        RegisterDelegateFunc(r, typeof(TService), factory, reuse, setup, ifAlreadyRegistered, serviceKey);

    /// <summary>Registers delegate with explicit arguments to be injected by container avoiding the ServiceLocator anti-pattern</summary>
    public static void RegisterDelegate<TDep1, TDep2, TDep3, TService>(
        this IRegistrator r, Func<TDep1, TDep2, TDep3, TService> factory,
        IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        RegisterDelegateFunc(r, typeof(TService), factory, reuse, setup, ifAlreadyRegistered, serviceKey);

    /// <summary>Registers delegate with explicit arguments to be injected by container avoiding the ServiceLocator anti-pattern</summary>
    public static void RegisterDelegate<TDep1, TDep2, TDep3, TDep4, TService>(
        this IRegistrator r, Func<TDep1, TDep2, TDep3, TDep4, TService> factory,
        IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        RegisterDelegateFunc(r, typeof(TService), factory, reuse, setup, ifAlreadyRegistered, serviceKey);

    /// <summary>Registers delegate with explicit arguments to be injected by container avoiding the ServiceLocator anti-pattern</summary>
    public static void RegisterDelegate<TDep1, TDep2, TDep3, TDep4, TDep5, TService>(
        this IRegistrator r, Func<TDep1, TDep2, TDep3, TDep4, TDep5, TService> factory,
        IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        RegisterDelegateFunc(r, typeof(TService), factory, reuse, setup, ifAlreadyRegistered, serviceKey);

    /// <summary>Registers delegate with explicit arguments to be injected by container avoiding the ServiceLocator anti-pattern</summary>
    public static void RegisterDelegate<TDep1, TDep2, TDep3, TDep4, TDep5, TDep6, TService>(
        this IRegistrator r, Func<TDep1, TDep2, TDep3, TDep4, TDep5, TDep6, TService> factory,
        IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        RegisterDelegateFunc(r, typeof(TService), factory, reuse, setup, ifAlreadyRegistered, serviceKey);

    /// <summary>Registers delegate with explicit arguments to be injected by container avoiding the ServiceLocator anti-pattern</summary>
    public static void RegisterDelegate<TDep1, TDep2, TDep3, TDep4, TDep5, TDep6, TDep7, TService>(
        this IRegistrator r, Func<TDep1, TDep2, TDep3, TDep4, TDep5, TDep6, TDep7, TService> factory,
        IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        RegisterDelegateFunc(r, typeof(TService), factory, reuse, setup, ifAlreadyRegistered, serviceKey);

    private const string InvokeMethodName = "Invoke";
    private static void RegisterDelegateFunc<TFunc>(IRegistrator r, Type serviceType,
        TFunc factory, IReuse reuse, Setup setup, IfAlreadyRegistered? ifAlreadyRegistered, object serviceKey)
    {
      var invokeMethod = typeof(TFunc).GetTypeInfo().GetDeclaredMethod(InvokeMethodName);
      var made = new Made(new FactoryMethod(invokeMethod, Constant(factory)), serviceType);
      r.Register(new ReflectionFactory(serviceType, reuse, made, setup),
          serviceType, serviceKey, ifAlreadyRegistered, isStaticallyChecked: true);
    }

    /// Minimizes the number of allocations when converting from Func to named delegate
    public static object ToFactoryDelegate<TService>(this Func<IResolverContext, TService> f, IResolverContext r) => f(r);

    /// Lifts the result to the factory delegate without allocations on capturing value in lambda closure
    public static object ToFactoryDelegate(this object result, IResolverContext _) => result;

    /// <summary>Registers a factory delegate for creating an instance of <paramref name="serviceType"/>.
    /// Delegate can use resolver context parameter to resolve any required dependencies, e.g.:
    /// <code lang="cs"><![CDATA[container.RegisterDelegate<ICar>(r => new Car(r.Resolve<IEngine>()))]]></code></summary>
    /// <remarks>IMPORTANT: The method should be used as the last resort only! Though powerful it is a black-box for container,
    /// which prevents diagnostics, plus it is easy to get memory leaks (due variables captured in delegate closure),
    /// and impossible to use in compile-time scenarios.
    /// Consider using <see cref="Made"/> instead:
    /// <code lang="cs"><![CDATA[container.Register<ICar>(Made.Of(() => new Car(Arg.Of<IEngine>())))]]></code>
    /// </remarks>
    public static void RegisterDelegate(this IRegistrator registrator,
        Type serviceType, Func<IResolverContext, object> factoryDelegate,
        IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
        object serviceKey = null)
    {
      if (serviceType.IsOpenGeneric())
        Throw.It(Error.ImpossibleToRegisterOpenGenericWithRegisterDelegate, serviceType);

      FactoryDelegate checkedDelegate = r => factoryDelegate(r)
          .ThrowIfNotInstanceOf(serviceType, Error.RegisteredDelegateResultIsNotOfServiceType);

      var factory = new DelegateFactory(checkedDelegate, reuse, setup);

      registrator.Register(factory, serviceType, serviceKey, ifAlreadyRegistered, isStaticallyChecked: false);
    }

    /// A special performant version mostly for integration with other libraries,
    /// that already check compatibility between delegate result and the service type
    public static void RegisterDelegate(this IRegistrator registrator,
        bool isChecked, Type serviceType, Func<IResolverContext, object> factoryDelegate,
        IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
        object serviceKey = null) =>
        registrator.Register(new DelegateFactory(factoryDelegate.ToFactoryDelegate, reuse, setup),
            serviceType, serviceKey, ifAlreadyRegistered, isStaticallyChecked: true);

    ///<summary>[Obsolete("Replaced with RegisterDelegate{Dep1...Dep2, R}()")]</summary>
    public static void RegisterDelegateDecorator<TService>(this IRegistrator registrator,
        Func<IResolverContext, Func<TService, TService>> getDecorator, Func<Request, bool> condition = null)
    {
      getDecorator.ThrowIfNull();
      registrator.RegisterDelegate<IResolverContext, TService, TService>(
          (r, service) => getDecorator(r)(service),
          setup: condition == null
              ? Setup.DecoratorWith(useDecorateeReuse: true)
              : Setup.DecoratorWith(condition, useDecorateeReuse: true));
    }

    /// Will become OBSOLETE! in the next major version:
    /// Please use `RegisterInstance` or `Use` method instead.
    public static void UseInstance<TService>(this IResolverContext r, TService instance,
        bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
        r.UseInstance(typeof(TService), instance, IfAlreadyRegistered.Replace, preventDisposal, weaklyReferenced, serviceKey);

    /// Will become OBSOLETE! in the next major version:
    /// Please use `RegisterInstance` or `Use` method instead.
    public static void UseInstance<TService>(this IRegistrator r, TService instance,
        bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
        r.UseInstance(typeof(TService), instance, IfAlreadyRegistered.Replace, preventDisposal, weaklyReferenced, serviceKey);

    /// Will become OBSOLETE! in the next major version:
    /// Please use `RegisterInstance` or `Use` method instead.
    public static void UseInstance<TService>(this IContainer c, TService instance,
        bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
        c.UseInstance(typeof(TService), instance, IfAlreadyRegistered.Replace, preventDisposal, weaklyReferenced, serviceKey);

    /// Will become OBSOLETE! in the next major version:
    /// Please use `RegisterInstance` or `Use` method instead.
    public static void UseInstance(this IResolverContext r, Type serviceType, object instance,
        bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
        r.UseInstance(serviceType, instance, IfAlreadyRegistered.Replace, preventDisposal, weaklyReferenced, serviceKey);

    /// Will become OBSOLETE! in the next major version:
    /// Please use `RegisterInstance` or `Use` method instead.
    public static void UseInstance(this IRegistrator r, Type serviceType, object instance,
        bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
        r.UseInstance(serviceType, instance, IfAlreadyRegistered.Replace, preventDisposal, weaklyReferenced, serviceKey);

    /// Will become OBSOLETE! in the next major version:
    /// Please use `RegisterInstance` or `Use` method instead.
    public static void UseInstance(this IContainer c, Type serviceType, object instance,
        bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
        c.UseInstance(serviceType, instance, IfAlreadyRegistered.Replace, preventDisposal, weaklyReferenced, serviceKey);

    /// Will become OBSOLETE! in the next major version:
    /// Please use `RegisterInstance` or `Use` method instead.
    public static void UseInstance<TService>(this IResolverContext r, TService instance, IfAlreadyRegistered ifAlreadyRegistered,
        bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
        r.UseInstance(typeof(TService), instance, ifAlreadyRegistered, preventDisposal, weaklyReferenced, serviceKey);

    /// Will become OBSOLETE! in the next major version:
    /// Please use `RegisterInstance` or `Use` method instead.
    public static void UseInstance(this IResolverContext r, Type serviceType, object instance, IfAlreadyRegistered ifAlreadyRegistered,
        bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
        r.UseInstance(serviceType, instance, ifAlreadyRegistered, preventDisposal, weaklyReferenced, serviceKey);

    /// Will become OBSOLETE! in the next major version:
    /// Please use `RegisterInstance` or `Use` method instead.
    public static void UseInstance(this IRegistrator r, Type serviceType, object instance, IfAlreadyRegistered ifAlreadyRegistered,
        bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
        r.UseInstance(serviceType, instance, ifAlreadyRegistered, preventDisposal, weaklyReferenced, serviceKey);

    /// <summary>
    /// Will become OBSOLETE in the next major version!
    /// Please use `RegisterInstance` or `Use` method instead.
    /// </summary>
    public static void UseInstance(this IContainer c, Type serviceType, object instance, IfAlreadyRegistered ifAlreadyRegistered,
        bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
        c.UseInstance(serviceType, instance, ifAlreadyRegistered, preventDisposal, weaklyReferenced, serviceKey);

    /// <summary>Adding the factory directly to scope for resolution</summary> 
    public static void Use<TService>(this IResolverContext r, Func<IResolverContext, TService> factory) =>
        r.Use(typeof(TService), factory.ToFactoryDelegate);

    /// <summary>Adding the instance directly to the scope for resolution</summary>
    public static void Use(this IResolverContext r, Type serviceType, object instance) =>
        r.Use(serviceType, instance.ToFactoryDelegate);

    /// <summary>Adding the instance directly to the scope for resolution</summary> 
    public static void Use<TService>(this IResolverContext r, TService instance) =>
        r.Use(typeof(TService), instance.ToFactoryDelegate);

    /// <summary>Adding the factory directly to the scope for resolution</summary>
    public static void Use<TService>(this IRegistrator r, Func<IResolverContext, TService> factory) =>
        r.Use(typeof(TService), factory.ToFactoryDelegate);

    /// <summary>Adding the instance directly to scope for resolution</summary>
    public static void Use(this IRegistrator r, Type serviceType, object instance) =>
        r.Use(serviceType, instance.ToFactoryDelegate);

    /// <summary>Adding the instance directly to scope for resolution</summary> 
    public static void Use<TService>(this IRegistrator r, TService instance) =>
        r.Use(typeof(TService), instance.ToFactoryDelegate);

    /// <summary>Adding the factory directly to scope for resolution</summary> 
    public static void Use<TService>(this IContainer c, Func<IResolverContext, TService> factory) =>
        ((IResolverContext)c).Use(typeof(TService), factory.ToFactoryDelegate);

    /// <summary>Adding the instance directly to scope for resolution</summary>
    public static void Use(this IContainer c, Type serviceType, object instance) =>
        ((IResolverContext)c).Use(serviceType, instance.ToFactoryDelegate);

    /// <summary>Adding the instance directly to scope for resolution</summary>
    public static void Use<TService>(this IContainer c, TService instance) =>
        ((IResolverContext)c).Use(typeof(TService), instance.ToFactoryDelegate);

    /// <summary>
    /// Registers initializing action that will be called after service is resolved 
    /// just before returning it to the caller. You can register multiple initializers for a single service.
    /// Or you can register initializer for the <see cref="Object"/> type to be applied 
    /// for all services and use <paramref name="condition"/> to specify the target services.
    /// Note: The initializer action has the same reuse as a initialized (decorated) service.
    /// </summary>
    public static void RegisterInitializer<TTarget>(this IRegistrator registrator,
        Action<TTarget, IResolverContext> initialize, Func<Request, bool> condition = null) =>
        registrator.RegisterInitializer<TTarget>(initialize, null, condition);

    /// <summary>
    /// Registers initializing action that will be called after service is resolved 
    /// just before returning it to the caller. You can register multiple initializers for a single service.
    /// Or you can register initializer for the <see cref="Object"/> type to be applied 
    /// for all services and use <paramref name="condition"/> to specify the target services.
    /// Note: You may specify a <paramref name="reuse"/> different from the initiliazed object enabling the
    /// <paramref name="initialize"/> action to run once (Singleton), run once-per-scope (Scoped), run always (Transient).
    /// </summary>
    public static void RegisterInitializer<TTarget>(this IRegistrator registrator,
        Action<TTarget, IResolverContext> initialize,
        IReuse reuse,
        Func<Request, bool> condition = null)
    {
      initialize.ThrowIfNull();

      registrator.Register<object>(
          reuse: reuse,
          made: Made.Of(
              r => _initializerMethod.MakeGenericMethod(typeof(TTarget), r.ServiceType),
              // specify ResolverContext as a parameter to prevent applying initializer for injected resolver too
              parameters: Parameters.Of
                  .Type<IResolverContext>(r => r.IsSingletonOrDependencyOfSingleton && !r.OpensResolutionScope ? r.Container.RootOrSelf() : r.Container)
                  .Type(initialize.ToFunc<Request, Action<TTarget, IResolverContext>>)),
          setup: Setup.DecoratorWith(
              r => r.ServiceType.IsAssignableTo<TTarget>() && (condition == null || condition(r)),
              useDecorateeReuse: true, // issue BitBucket #230 - ensures the initialization to happen once on construction 
              preventDisposal: true)); // issue #215 - ensures that the initialized / decorated object does not added for the disposal twice
    }

    private static readonly MethodInfo _initializerMethod =
        typeof(Registrator).SingleMethod(nameof(Initializer), includeNonPublic: true);

    internal static TService Initializer<TTarget, TService>(
        TService service, IResolverContext resolver, Action<TTarget, IResolverContext> initialize) where TService : TTarget
    {
      initialize(service, resolver);
      return service;
    }

    /// <summary>Registers dispose action for reused target service.</summary>
    public static void RegisterDisposer<TService>(this IRegistrator registrator,
        Action<TService> dispose, Func<Request, bool> condition = null)
    {
      dispose.ThrowIfNull();

      var disposerKey = new object();

      registrator.RegisterDelegate(_ => new Disposer<TService>(dispose),
          serviceKey: disposerKey,
          // tracking instead of parent reuse, so that I can use one disposer for multiple services
          setup: Setup.With(trackDisposableTransient: true));

      var disposerType = typeof(Disposer<>).MakeGenericType(typeof(TService));
      registrator.Register<object>(
          made: Made.Of(
              r => disposerType.SingleMethod("TrackForDispose").MakeGenericMethod(r.ServiceType),
              ServiceInfo.Of(disposerType, serviceKey: disposerKey)),
          setup: Setup.DecoratorWith(
              r => r.ServiceType.IsAssignableTo<TService>() && (condition == null || condition(r)),
              useDecorateeReuse: true));
    }

    internal sealed class Disposer<T> : IDisposable
    {
      private readonly Action<T> _dispose;
      private int _state;
      private const int TRACKED = 1, DISPOSED = 2;
      private T _item;

      public Disposer(Action<T> dispose)
      {
        _dispose = dispose;
      }

      public S TrackForDispose<S>(S item) where S : T
      {
        if (Interlocked.CompareExchange(ref _state, TRACKED, 0) != 0)
          Throw.It(Error.DisposerTrackForDisposeError, _state == TRACKED ? " tracked" : "disposed");
        _item = item;
        return item;
      }

      public void Dispose()
      {
        if (Interlocked.CompareExchange(ref _state, DISPOSED, TRACKED) != TRACKED)
          return;
        var item = _item;
        if (item != null)
        {
          _dispose(item);
          _item = default(T);
        }
      }
    }

    /// <summary>Returns true if <paramref name="serviceType"/> is registered in container OR
    /// its open generic definition is registered in container.
    /// The additional implementation factory <paramref name="condition"/> may be specified to narrow the search.</summary>
    public static bool IsRegistered(this IRegistrator registrator, Type serviceType,
        object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null) =>
        registrator.IsRegistered(serviceType, serviceKey, factoryType, condition);

    /// <summary>Returns true if <typeparamref name="TService"/> is registered in container OR
    /// its open generic definition is registered in container.
    /// The additional implementation factory <paramref name="condition"/> may be specified to narrow the search.</summary>
    public static bool IsRegistered<TService>(this IRegistrator registrator,
        object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null) =>
        registrator.IsRegistered(typeof(TService), serviceKey, factoryType, condition);

    /// <summary>Removes specified registration from container.
    /// It also tries to remove the cached resolutions for the removed registration, But it may not work depending on context.
    /// Check the docs for more info: https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/UnregisterAndResolutionCache.md </summary>
    public static void Unregister(this IRegistrator registrator, Type serviceType,
        object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null) =>
        registrator.Unregister(serviceType, serviceKey, factoryType, condition);

    /// <summary>Removes specified registration from container.
    /// It also tries to remove the cached resolutions for the removed registration, But it may not work depending on context.
    /// Check the docs for more info: https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/UnregisterAndResolutionCache.md </summary>
    public static void Unregister<TService>(this IRegistrator registrator,
        object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null) =>
        registrator.Unregister(typeof(TService), serviceKey, factoryType, condition);

    /// <summary>Registers new service type with factory for registered service type.
    /// Throw if no such registered service type in container.</summary>
    /// <param name="registrator">Registrator</param> <param name="serviceType">New service type.</param>
    /// <param name="registeredServiceType">Existing registered service type.</param>
    /// <param name="ifAlreadyRegistered">The registration to overwrite or preserve the already registered service</param>
    /// <param name="serviceKey">(optional)</param> <param name="registeredServiceKey">(optional)</param>
    /// <param name="factoryType">(optional) By default is <see cref="FactoryType.Service"/></param>
    public static void RegisterMapping(this IRegistrator registrator, Type serviceType, Type registeredServiceType,
        IfAlreadyRegistered? ifAlreadyRegistered, object serviceKey = null, object registeredServiceKey = null, FactoryType factoryType = FactoryType.Service)
    {
      var factories = registrator.GetRegisteredFactories(registeredServiceType, registeredServiceKey, factoryType);

      if (factories.IsNullOrEmpty())
        Throw.It(Error.RegisterMappingNotFoundRegisteredService, registeredServiceType, registeredServiceKey);

      if (factories.Length > 1)
        Throw.It(Error.RegisterMappingUnableToSelectFromMultipleFactories, serviceType, serviceKey, factories);

      registrator.Register(factories[0], serviceType, serviceKey, ifAlreadyRegistered, false);
    }

    /// <summary>Registers new service type with factory for registered service type.
    /// Throw if no such registered service type in container.</summary>
    public static void RegisterMapping(this IRegistrator registrator, Type serviceType, Type registeredServiceType,
        object serviceKey = null, object registeredServiceKey = null, FactoryType factoryType = FactoryType.Service) =>
        registrator.RegisterMapping(serviceType, registeredServiceType, null, serviceKey, registeredServiceKey, factoryType);

    /// <summary>Registers new service type with factory for registered service type.
    /// Throw if no such registered service type in container.</summary>
    /// <param name="registrator">Registrator</param>
    /// <typeparam name="TService">New service type.</typeparam>
    /// <typeparam name="TRegisteredService">Existing registered service type.</typeparam>
    /// <param name="serviceKey">(optional)</param> <param name="registeredServiceKey">(optional)</param>
    /// <param name="factoryType">(optional) By default is <see cref="FactoryType.Service"/></param>
    public static void RegisterMapping<TService, TRegisteredService>(this IRegistrator registrator,
        object serviceKey = null, object registeredServiceKey = null, FactoryType factoryType = FactoryType.Service) =>
        registrator.RegisterMapping(typeof(TService), typeof(TRegisteredService), null, serviceKey, registeredServiceKey);

    /// <summary>Registers new service type with factory for registered service type.
    /// Throw if no such registered service type in container.</summary>
    /// <param name="container">Container</param>
    /// <typeparam name="TService">New service type.</typeparam>
    /// <typeparam name="TRegisteredService">Existing registered service type.</typeparam>
    /// <param name="ifAlreadyRegistered">The registration to overwrite or preserve the already registered service</param>
    /// <param name="serviceKey">(optional)</param> <param name="registeredServiceKey">(optional)</param>
    public static void RegisterMapping<TService, TRegisteredService>(this IContainer container,
        IfAlreadyRegistered ifAlreadyRegistered, object serviceKey = null, object registeredServiceKey = null) =>
        Registrator.RegisterMapping(container,
            typeof(TService), typeof(TRegisteredService), ifAlreadyRegistered, serviceKey, registeredServiceKey);

    /// <summary>Register a service without implementation which can be provided later in terms
    /// of normal registration with `IfAlreadyRegistered.Replace` parameter.
    /// When the implementation is still not provided when the placeholder service is accessed, then the exception will be thrown.
    /// This feature allows you to postpone the decision on implementation until it is later known.</summary>
    /// <remarks>Internally the empty factory is registered with the setup `asResolutionCall: true`.
    /// That means, instead of placing service instance into graph expression we put here redirecting call to
    /// container Resolve.</remarks>
    public static void RegisterPlaceholder(this IRegistrator registrator, Type serviceType,
        IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        registrator.Register(FactoryPlaceholder.Default, serviceType, serviceKey, ifAlreadyRegistered, true);

    /// <summary>Register a service without implementation which can be provided later in terms
    /// of normal registration with `IfAlreadyRegistered.Replace` parameter.
    /// When the implementation is still not provided when the placeholder service is accessed, then the exception will be thrown.
    /// This feature allows you to postpone the decision on implementation until it is later known.</summary>
    /// <remarks>Internally the empty factory is registered with the setup `asResolutionCall: true`.
    /// That means, instead of placing service instance into graph expression we put here redirecting call to
    /// container Resolve.</remarks>
    public static void RegisterPlaceholder<TService>(this IRegistrator registrator,
        IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
        registrator.RegisterPlaceholder(typeof(TService), ifAlreadyRegistered, serviceKey);
  }

  /// <summary>Extension methods for <see cref="IResolver"/>.</summary>
  public static class Resolver
  {
    internal static readonly MethodInfo ResolveFastMethod =
        typeof(IResolver).Method(nameof(IResolver.Resolve), typeof(Type), typeof(IfUnresolved));

    internal static readonly MethodInfo ResolveMethod =
        typeof(IResolver).Method(nameof(IResolver.Resolve), typeof(Type), typeof(object),
            typeof(IfUnresolved), typeof(Type), typeof(Request), typeof(object[]));

    internal static readonly MethodInfo ResolveManyMethod =
        typeof(IResolver).GetTypeInfo().GetDeclaredMethod(nameof(IResolver.ResolveMany));

    /// <summary>Resolves instance of service type from container. Throws exception if unable to resolve.</summary>
    public static object Resolve(this IResolver resolver, Type serviceType) =>
        resolver.Resolve(serviceType, IfUnresolved.Throw);

    /// <summary>Resolves instance of service type from container.</summary>
    public static object Resolve(this IResolver resolver, Type serviceType, IfUnresolved ifUnresolved) =>
        resolver.Resolve(serviceType, ifUnresolved);

    /// <summary>Resolves instance of type TService from container.</summary>
    public static TService Resolve<TService>(this IResolver resolver,
        IfUnresolved ifUnresolved = IfUnresolved.Throw) =>
        (TService)resolver.Resolve(typeof(TService), ifUnresolved);

    /// <summary>Tries to resolve instance of service type from container.</summary>
    public static object Resolve(this IResolver resolver, Type serviceType, bool ifUnresolvedReturnDefault) =>
        resolver.Resolve(serviceType, ifUnresolvedReturnDefault ? IfUnresolved.ReturnDefault : IfUnresolved.Throw);

    /// <summary>Tries to resolve instance of TService from container.</summary>
    public static object Resolve<TService>(this IResolver resolver, bool ifUnresolvedReturnDefault) =>
        resolver.Resolve(typeof(TService), ifUnresolvedReturnDefault);

    /// <summary>Returns instance of <paramref name="serviceType"/> searching for <paramref name="requiredServiceType"/>.
    /// In case of <paramref name="serviceType"/> being generic wrapper like Func, Lazy, IEnumerable, etc. 
    /// <paramref name="requiredServiceType"/> allow you to specify wrapped service type.</summary>
    /// <example><code lang="cs"><![CDATA[
    ///     container.Register<IService, Service>();
    ///     var services = container.Resolve(typeof(IEnumerable<object>), typeof(IService));
    /// ]]></code></example>
    public static object Resolve(this IResolver resolver, Type serviceType, Type requiredServiceType,
        IfUnresolved ifUnresolved = IfUnresolved.Throw, object[] args = null, object serviceKey = null) =>
        resolver.Resolve(serviceType, serviceKey, ifUnresolved, requiredServiceType, Request.Empty, args);

    /// <summary>Returns instance of <typeparamref name="TService"/> searching for <paramref name="requiredServiceType"/>.
    /// In case of <typeparamref name="TService"/> being generic wrapper like Func, Lazy, IEnumerable, etc. 
    /// <paramref name="requiredServiceType"/> allow you to specify wrapped service type.</summary>
    /// <example><code lang="cs"><![CDATA[
    ///     container.Register<IService, Service>();
    ///     var services = container.Resolve<IEnumerable<object>>(typeof(IService));
    /// ]]></code></example>
    public static TService Resolve<TService>(this IResolver resolver, Type requiredServiceType,
        IfUnresolved ifUnresolved = IfUnresolved.Throw, object[] args = null, object serviceKey = null) =>
        (TService)resolver.Resolve(typeof(TService), serviceKey, ifUnresolved, requiredServiceType, Request.Empty, args);

    /// <summary>Returns instance of <typeparamref name="TService"/> searching for <typeparamref name="TRequiredService"/>.
    /// In case of <typeparamref name="TService"/> being generic wrapper like Func, Lazy, IEnumerable, etc. 
    /// <typeparamref name="TRequiredService"/> allow you to specify wrapped service type.</summary>
    /// <example><code lang="cs"><![CDATA[
    ///     container.Register<IService, Service>();
    ///     var services = container.Resolve<IEnumerable<object>, IService>();
    /// ]]></code></example>
    public static TService Resolve<TService, TRequiredService>(this IResolver resolver,
        IfUnresolved ifUnresolved = IfUnresolved.Throw, object[] args = null, object serviceKey = null) =>
        (TService)resolver.Resolve(typeof(TService), serviceKey, ifUnresolved, typeof(TRequiredService), Request.Empty, args);

    /// <summary>Returns instance of <paramref name="serviceType"/> searching for <paramref name="requiredServiceType"/>.
    /// In case of <paramref name="serviceType"/> being generic wrapper like Func, Lazy, IEnumerable, etc., <paramref name="requiredServiceType"/>
    /// could specify wrapped service type.</summary>
    /// <remarks>Using <paramref name="requiredServiceType"/> implicitly support Covariance for generic wrappers even in .Net 3.5.</remarks>
    /// <example><code lang="cs"><![CDATA[
    ///     container.Register<IService, Service>();
    ///     var services = container.Resolve(typeof(Lazy<object>), "someKey", requiredServiceType: typeof(IService));
    /// ]]></code></example>
    public static object Resolve(this IResolver resolver, Type serviceType, object serviceKey,
        IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
        object[] args = null) =>
        resolver.Resolve(serviceType, serviceKey, ifUnresolved, requiredServiceType, Request.Empty, args);

    /// <summary>Returns instance of <typeparamref name="TService"/> type.</summary>
    /// <typeparam name="TService">The type of the requested service.</typeparam>
    /// <returns>The requested service instance.</returns>
    /// <remarks>Using <paramref name="requiredServiceType"/> implicitly support Covariance for generic wrappers even in .Net 3.5.</remarks>
    public static TService Resolve<TService>(this IResolver resolver, object serviceKey,
        IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
        object[] args = null) =>
        (TService)resolver.Resolve(typeof(TService), serviceKey, ifUnresolved, requiredServiceType, Request.Empty, args);

    /// <summary>Resolves the service supplying all or some of its dependencies 
    /// (including nested) with the <paramref name="args"/>. The rest of dependencies is injected from
    /// container.</summary>
    public static object Resolve(this IResolver resolver, Type serviceType, object[] args,
        IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
        object serviceKey = null) =>
        resolver.Resolve(serviceType, serviceKey, ifUnresolved, requiredServiceType, Request.Empty, args);

    /// <summary>Resolves the service supplying all or some of its dependencies 
    /// (including nested) with the <paramref name="args"/>. The rest of dependencies is injected from
    /// container.</summary>
    public static TService Resolve<TService>(this IResolver resolver, object[] args,
        IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
        object serviceKey = null) =>
        (TService)resolver.Resolve(typeof(TService), serviceKey, ifUnresolved, requiredServiceType, Request.Empty, args);

    /// <summary>Returns all registered services instances including all keyed and default registrations.
    /// Use <paramref name="behavior"/> to return either all registered services at the moment of resolve (dynamic fresh view) or
    /// the same services that were returned with first <see cref="ResolveMany{TService}"/> call (fixed view).</summary>
    /// <typeparam name="TService">Return collection item type. 
    /// It denotes registered service type if <paramref name="requiredServiceType"/> is not specified.</typeparam>
    /// <remarks>The same result could be achieved by directly calling:
    /// <code lang="cs"><![CDATA[
    ///     container.Resolve<LazyEnumerable<IService>>();  // for dynamic result - default behavior
    ///     container.Resolve<IService[]>();                // for fixed array
    ///     container.Resolve<IEnumerable<IService>>();     // same as fixed array
    /// ]]></code>
    /// </remarks>
    public static IEnumerable<TService> ResolveMany<TService>(this IResolver resolver,
        Type requiredServiceType = null, ResolveManyBehavior behavior = ResolveManyBehavior.AsLazyEnumerable,
        object[] args = null, object serviceKey = null) =>
        behavior == ResolveManyBehavior.AsLazyEnumerable
            ? resolver.ResolveMany(typeof(TService), serviceKey, requiredServiceType, Request.Empty, args).Cast<TService>()
            : resolver.Resolve<IEnumerable<TService>>(serviceKey, IfUnresolved.Throw, requiredServiceType, args);

    /// <summary>Returns all registered services as objects, including all keyed and default registrations.</summary>
    public static IEnumerable<object> ResolveMany(this IResolver resolver, Type serviceType,
        ResolveManyBehavior behavior = ResolveManyBehavior.AsLazyEnumerable,
        object[] args = null, object serviceKey = null) =>
        resolver.ResolveMany<object>(serviceType, behavior, args, serviceKey);

    /// <summary>Creates a service by injecting its parameters registered in the container but without registering the service itself in the container.</summary>
    public static object New(this IResolver resolver, Type concreteType, Made made = null,
        RegistrySharing registrySharing = RegistrySharing.CloneButKeepCache) =>
        resolver.Resolve<IContainer>().New(concreteType, setup: null, made, registrySharing);

    /// <summary>Creates a service by injecting its parameters registered in the container but without registering the service itself in the container.</summary>
    public static T New<T>(this IResolver resolver, Made made = null,
        RegistrySharing registrySharing = RegistrySharing.CloneButKeepCache) =>
        (T)resolver.New(typeof(T), made, registrySharing);

    /// <summary>Creates a service by injecting its parameters registered in the container but without registering the service itself in the container.</summary>
    public static T New<T>(this IResolver resolver, Made.TypedMade<T> made,
        RegistrySharing registrySharing = RegistrySharing.CloneButKeepCache) =>
        (T)resolver.New(typeof(T), made, registrySharing);

    internal static readonly ConstructorInfo ResolutionScopeNameCtor =
        typeof(ResolutionScopeName).GetTypeInfo().DeclaredConstructors.First();

    private static readonly ConstantExpression _ifUnresolvedThrowExpr = Constant(IfUnresolved.Throw);
    private static readonly ConstantExpression _nullTypeExpr = Constant(null, typeof(Type));
    private static readonly ConstantExpression _nullExpr = Constant(null, typeof(object));

    internal static Expression CreateResolutionExpression(Request request,
        bool openResolutionScope = false, bool asResolutionCall = false)
    {
      if (request.Rules.DependencyResolutionCallExprs != null &&
          request.Factory != null && !request.Factory.HasRuntimeState)
        PopulateDependencyResolutionCallExpressions(request);

      var container = request.Container;
      var serviceType = request.ServiceType;

      var serviceTypeExpr = Constant(serviceType, typeof(Type));

      var details = request._serviceInfo.Details;

      var ifUnresolvedExpr = details.IfUnresolved == IfUnresolved.Throw
          ? _ifUnresolvedThrowExpr
          : Constant(details.IfUnresolved, typeof(IfUnresolved));

      var requiredServiceTypeExpr = details.RequiredServiceType == null
          ? _nullTypeExpr
          : Constant(details.RequiredServiceType, typeof(Type));

      var serviceKeyExpr = details.ServiceKey == null
          ? _nullExpr
          : container.GetConstantExpression(details.ServiceKey, typeof(object));

      Expression resolverExpr;
      if (!openResolutionScope)
      {
        resolverExpr = ResolverContext.GetRootOrSelfExpr(request);
      }
      else
      {
        // Generates the code below. That means the service opening the scope is scoped to this scope.
        //
        // r => r.OpenScope(new ResolutionScopeName(serviceType, serviceKey), trackInParent: true)
        //       .Resolve(serviceType, serviceKey)
        //
        var actualServiceTypeExpr = Constant(request.GetActualServiceType(), typeof(Type));
        var scopeNameExpr = Expression.New(ResolutionScopeNameCtor, actualServiceTypeExpr, serviceKeyExpr);
        var trackInParent = Constant(true);

        resolverExpr = Call(ResolverContext.OpenScopeMethod,
            FactoryDelegateCompiler.ResolverContextParamExpr, scopeNameExpr, trackInParent);
      }

      var parentFlags = default(RequestFlags);
      if (openResolutionScope)
        parentFlags |= RequestFlags.OpensResolutionScope;
      if (asResolutionCall || (request.Flags & RequestFlags.StopRecursiveDependencyCheck) != 0)
        parentFlags |= RequestFlags.StopRecursiveDependencyCheck;

      // Only parent is converted to be passed to Resolve.
      // The current request is formed by rest of Resolve parameters.
      var preResolveParentExpr = container.GetRequestExpression(request.DirectParent, parentFlags);

      var resolveCallExpr = Call(resolverExpr, ResolveMethod, serviceTypeExpr, serviceKeyExpr,
          ifUnresolvedExpr, requiredServiceTypeExpr, preResolveParentExpr, request.GetInputArgsExpr());

      if (serviceType == typeof(object))
        return resolveCallExpr;

      return Convert(resolveCallExpr, serviceType);
    }

    private static void PopulateDependencyResolutionCallExpressions(Request request)
    {
      // Actually calls nested Resolve and stores produced expression in collection inside the container Rules.
      // Stops on recursive dependency, e.g. 
      // `new A(new Lazy<B>(r => r.Resolve<B>())` and `new B(new A())`
      for (var p = request.DirectParent; !p.IsEmpty; p = p.DirectParent)
        if (p.FactoryID == request.FactoryID)
          return;

      var factory = request.Container.ResolveFactory(request);
      if (factory == null || factory is FactoryPlaceholder)
        return;

      // Prevents infinite recursion when generating the resolution dependency #579
      if ((request.Flags & RequestFlags.IsGeneratedResolutionDependencyExpression) != 0)
        return;

      request.Flags |= RequestFlags.IsGeneratedResolutionDependencyExpression;

      var factoryExpr = factory.GetExpressionOrDefault(request)?.NormalizeExpression();
      if (factoryExpr == null)
        return;

      request.Container.Rules.DependencyResolutionCallExprs.Swap(request, factoryExpr,
          (x, req, facExpr) => x.AddOrUpdate(req, facExpr
#if SUPPORTS_FAST_EXPRESSION_COMPILER
                        .ToExpression()
#endif
                ));
    }
  }

  /// <summary>Specifies result of <see cref="Resolver.ResolveMany{TService}"/>: either dynamic(lazy) or fixed view.</summary>
  public enum ResolveManyBehavior
  {
    /// <summary>Lazy/dynamic item resolve.</summary>
    AsLazyEnumerable,
    /// <summary>Fixed array of item at time of resolve, newly registered/removed services won't be listed.</summary>
    AsFixedArray
  }

  /// <summary>Controls the registry change</summary>
  public enum IsRegistryChangePermitted
  {
    /// <summary>Change is permitted - the default setting</summary>
    Permitted,
    /// <summary>Throws the error for the new registration</summary>
    Error,
    /// <summary>Ignores the next registration</summary>
    Ignored
  }

  /// <summary>Provides information required for service resolution: service type
  /// and optional <see cref="ServiceDetails"/></summary>
  public interface IServiceInfo
  {
    /// <summary>The required piece of info: service type.</summary>
    Type ServiceType { get; }

    /// <summary>Additional optional details: service key, if-unresolved policy, required service type.</summary>
    ServiceDetails Details { get; }

    /// <summary>Creates info from service type and details.</summary>
    IServiceInfo Create(Type serviceType, ServiceDetails details);
  }

  /// <summary>Provides optional service resolution details: service key, required service type, what return when service is unresolved,
  /// default value if service is unresolved, custom service value.</summary>
  public class ServiceDetails
  {
    /// Default details if not specified, use default setting values, e.g. <see cref="Injector.IfUnresolved.Throw"/>
    public static readonly ServiceDetails Default =
        new ServiceDetails(null, IfUnresolved.Throw, null, null, null, null, false);

    /// Default details with <see cref="Injector.IfUnresolved.ReturnDefault"/> option.
    public static readonly ServiceDetails IfUnresolvedReturnDefault =
        new ServiceDetails(null, IfUnresolved.ReturnDefault, null, null, null, null, false);

    /// Default details with <see cref="Injector.IfUnresolved.ReturnDefaultIfNotRegistered"/> option.
    public static readonly ServiceDetails IfUnresolvedReturnDefaultIfNotRegistered =
        new ServiceDetails(null, IfUnresolved.ReturnDefaultIfNotRegistered, null, null, null, null, false);

    /// <summary>Creates new details out of provided settings, or returns default if all settings have default value.</summary>
    public static ServiceDetails Of(Type requiredServiceType = null,
        object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw,
        object defaultValue = null, string metadataKey = null, object metadata = null)
    {
      if (defaultValue != null)
      {
        // IfUnresolved.Throw does not make sense when default value is provided, so normalizing it to ReturnDefault
        if (ifUnresolved == IfUnresolved.Throw)
          ifUnresolved = IfUnresolved.ReturnDefault;
      }
      else if (requiredServiceType == null && serviceKey == null &&
               metadataKey == null && metadata == null)
      {
        if (ifUnresolved == IfUnresolved.Throw)
          return Default;
        if (ifUnresolved == IfUnresolved.ReturnDefault)
          return IfUnresolvedReturnDefault;
        if (ifUnresolved == IfUnresolved.ReturnDefaultIfNotRegistered)
          return IfUnresolvedReturnDefaultIfNotRegistered;
      }

      return new ServiceDetails(requiredServiceType, ifUnresolved,
          serviceKey, metadataKey, metadata, defaultValue, hasCustomValue: false);
    }

    /// <summary>Sets custom value for service. This setting is orthogonal to the rest.
    /// Using default value with invalid ifUnresolved.Throw option to indicate custom value.</summary>
    public static ServiceDetails Of(object value) =>
        new ServiceDetails(null, IfUnresolved.Throw, null, null, null, value, hasCustomValue: true);

    /// <summary>Service type to search in registry. Should be assignable to user requested service type.</summary>
    public readonly Type RequiredServiceType;

    /// <summary>Service key provided with registration.</summary>
    public readonly object ServiceKey;

    /// <summary>Metadata key to find in metadata dictionary in resolved service.</summary>
    public readonly string MetadataKey;

    /// <summary>Metadata value to find in resolved service.</summary>
    public readonly object Metadata;

    /// <summary>Policy to deal with unresolved request.</summary>
    public readonly IfUnresolved IfUnresolved;

    /// <summary>Indicates that the custom value is specified.</summary>
    public readonly bool HasCustomValue;

    /// <summary>Either default or custom value depending on <see cref="IfUnresolved"/> setting.</summary>
    private readonly object _value;

    /// <summary>Value to use in case <see cref="IfUnresolved"/> is set to not Throw.</summary>
    public object DefaultValue => IfUnresolved != IfUnresolved.Throw ? _value : null;

    /// <summary>Custom value specified for dependency. The IfUnresolved.Throw is the marker of custom value comparing to default value.</summary>
    public object CustomValue => IfUnresolved == IfUnresolved.Throw ? _value : null;

    /// <summary>Pretty prints service details to string for debugging and errors.</summary> <returns>Details string.</returns>
    public override string ToString()
    {
      var s = new StringBuilder();

      if (HasCustomValue)
        return s.Append("{CustomValue=").Print(CustomValue ?? "null").Append("}").ToString();

      if (RequiredServiceType != null)
        s.Append("RequiredServiceType=").Print(RequiredServiceType);
      if (ServiceKey != null)
        (s.Length == 0 ? s.Append('{') : s.Append(", ")).Append("ServiceKey=").Print(ServiceKey);
      if (MetadataKey != null || Metadata != null)
        (s.Length == 0 ? s.Append('{') : s.Append(", ")).Append("Metadata=").Append(MetadataKey.Pair(Metadata));
      if (IfUnresolved != IfUnresolved.Throw)
      {
        s = (s.Length == 0 ? s.Append('{') : s.Append(", ")).Print(IfUnresolved);
        s = _value == null ? s : s.Append(", DefaultValue=").Print(_value);
      }

      return (s.Length == 0 ? s : s.Append('}')).ToString();
    }

    private ServiceDetails(Type requiredServiceType, IfUnresolved ifUnresolved,
        object serviceKey, string metadataKey, object metadata,
        object value, bool hasCustomValue)
    {
      RequiredServiceType = requiredServiceType;
      IfUnresolved = ifUnresolved;
      ServiceKey = serviceKey;
      MetadataKey = metadataKey;
      Metadata = metadata;
      _value = value;
      HasCustomValue = hasCustomValue;
    }
  }

  /// <summary>Contains tools for combining or propagating of <see cref="IServiceInfo"/> independent of its concrete implementations.</summary>
  public static class ServiceInfoTools
  {
    /// <summary>Creates service info with new type but keeping the details.</summary>
    public static IServiceInfo With(this IServiceInfo source, Type serviceType) =>
        source.Create(serviceType, source.Details);

    /// <summary>Creates new info with new IfUnresolved behavior or returns the original info if behavior is not different,
    /// or the passed info is not a <see cref="ServiceDetails.HasCustomValue"/>.</summary>
    public static IServiceInfo WithIfUnresolved(this IServiceInfo source, IfUnresolved ifUnresolved)
    {
      var details = source.Details;
      if (details.IfUnresolved == ifUnresolved || details.HasCustomValue)
        return source;

      if (details == ServiceDetails.Default)
        details = ifUnresolved == IfUnresolved.ReturnDefault
            ? ServiceDetails.IfUnresolvedReturnDefault
            : ServiceDetails.IfUnresolvedReturnDefaultIfNotRegistered;
      else
        details = ServiceDetails.Of(details.RequiredServiceType, details.ServiceKey,
            ifUnresolved, details.DefaultValue, details.MetadataKey, details.Metadata);

      return source.Create(source.ServiceType, details);
    }

    // todo: Should be renamed or better to be removed, the whole operation should be hidden behind abstraction
    /// <summary>Combines service info with details. The main goal is to combine service and required service type.</summary>
    public static T WithDetails<T>(this T serviceInfo, ServiceDetails details)
        where T : IServiceInfo
    {
      details = details ?? ServiceDetails.Default;
      var sourceDetails = serviceInfo.Details;
      if (!details.HasCustomValue &&
          sourceDetails != ServiceDetails.Default &&
          sourceDetails != details)
      {
        var serviceKey = details.ServiceKey ?? sourceDetails.ServiceKey;
        var metadataKey = details.MetadataKey ?? sourceDetails.MetadataKey;
        var metadata = metadataKey == details.MetadataKey ? details.Metadata : sourceDetails.Metadata;
        var defaultValue = details.DefaultValue ?? sourceDetails.DefaultValue;

        details = ServiceDetails.Of(details.RequiredServiceType, serviceKey,
            details.IfUnresolved, defaultValue, metadataKey, metadata);
      }

      var serviceType = serviceInfo.ServiceType;
      var requiredServiceType = details.RequiredServiceType;

      if (requiredServiceType != null && requiredServiceType == serviceType)
        details = ServiceDetails.Of(null,
            details.ServiceKey, details.IfUnresolved, details.DefaultValue,
            details.MetadataKey, details.Metadata);

      // if service type unchanged and details absent, or details are the same return original info, otherwise create new one
      return serviceType == serviceInfo.ServiceType
          && (details == null || details == serviceInfo.Details)
          ? serviceInfo
          : (T)serviceInfo.Create(serviceType, details);
    }

    /// <summary>Enables propagation/inheritance of info between dependency and its owner:
    /// for instance <see cref="ServiceDetails.RequiredServiceType"/> for wrappers.</summary>
    public static IServiceInfo InheritInfoFromDependencyOwner(this IServiceInfo dependency,
        IServiceInfo owner, IContainer container, FactoryType ownerType = FactoryType.Service)
    {
      var ownerDetails = owner.Details;
      if (ownerDetails == null || ownerDetails == ServiceDetails.Default)
        return dependency;

      var dependencyDetails = dependency.Details;

      var ownerIfUnresolved = ownerDetails.IfUnresolved;
      var ifUnresolved = dependencyDetails.IfUnresolved;
      if (ownerIfUnresolved == IfUnresolved.ReturnDefault) // ReturnDefault is always inherited
        ifUnresolved = ownerIfUnresolved;

      var serviceType = dependency.ServiceType;
      var requiredServiceType = dependencyDetails.RequiredServiceType;
      var ownerRequiredServiceType = ownerDetails.RequiredServiceType;

      var serviceKey = dependencyDetails.ServiceKey;
      var metadataKey = dependencyDetails.MetadataKey;
      var metadata = dependencyDetails.Metadata;

      // Inherit some things through wrappers and decorators
      if (ownerType == FactoryType.Wrapper ||
          ownerType == FactoryType.Decorator &&
          container.GetWrappedType(serviceType, requiredServiceType).IsAssignableTo(owner.ServiceType))
      {
        if (ownerIfUnresolved == IfUnresolved.ReturnDefaultIfNotRegistered)
          ifUnresolved = ownerIfUnresolved;

        if (serviceKey == null)
          serviceKey = ownerDetails.ServiceKey;

        if (metadataKey == null && metadata == null)
        {
          metadataKey = ownerDetails.MetadataKey;
          metadata = ownerDetails.Metadata;
        }
      }

      if (ownerType != FactoryType.Service && ownerRequiredServiceType != null &&
          requiredServiceType == null) // if only dependency does not have its own
        requiredServiceType = ownerRequiredServiceType;

      if (serviceKey == dependencyDetails.ServiceKey &&
          metadataKey == dependencyDetails.MetadataKey && metadata == dependencyDetails.Metadata &&
          ifUnresolved == dependencyDetails.IfUnresolved && requiredServiceType == dependencyDetails.RequiredServiceType)
        return dependency;

      if (serviceType == requiredServiceType)
        requiredServiceType = null;

      var serviceDetails = ServiceDetails.Of(requiredServiceType,
          serviceKey, ifUnresolved, dependencyDetails.DefaultValue,
          metadataKey, metadata);

      return dependency.Create(serviceType, serviceDetails);
    }

    /// <summary>Returns required service type if it is specified and assignable to service type,
    /// otherwise returns service type.</summary>
    public static Type GetActualServiceType(this IServiceInfo info)
    {
      var requiredServiceType = info.Details.RequiredServiceType;
      return requiredServiceType != null && requiredServiceType.IsAssignableTo(info.ServiceType)
          ? requiredServiceType : info.ServiceType;
    }

    /// <summary>Appends info string representation into provided builder.</summary>
    public static StringBuilder Print(this StringBuilder s, IServiceInfo info)
    {
      s.Print(info.ServiceType);
      var details = info.Details.ToString();
      return details == string.Empty ? s : s.Append(' ').Append(details);
    }
  }

  /// <summary>Represents custom or resolution root service info, there is separate representation for parameter,
  /// property and field dependencies.</summary>
  public class ServiceInfo : IServiceInfo
  {
    /// <summary>Empty service info for convenience.</summary>
    public static readonly IServiceInfo Empty = new ServiceInfo(null);

    /// <summary>Creates info out of provided settings</summary>
    public static ServiceInfo Of(Type serviceType,
        IfUnresolved ifUnresolved = IfUnresolved.Throw, object serviceKey = null) =>
        Of(serviceType, null, ifUnresolved, serviceKey);

    /// <summary>Creates info out of provided settings</summary>
    public static ServiceInfo Of(Type serviceType, Type requiredServiceType,
        IfUnresolved ifUnresolved = IfUnresolved.Throw, object serviceKey = null,
        string metadataKey = null, object metadata = null)
    {
      (serviceType ?? requiredServiceType).ThrowIfNull();

      // remove unnecessary details if service and required type are the same
      if (serviceType == requiredServiceType)
        requiredServiceType = null;

      return serviceKey == null && requiredServiceType == null
          && metadataKey == null && metadata == null
          ? (ifUnresolved == IfUnresolved.Throw ? new ServiceInfo(serviceType)
              : ifUnresolved == IfUnresolved.ReturnDefault ? new WithDetails(serviceType, ServiceDetails.IfUnresolvedReturnDefault)
              : new WithDetails(serviceType, ServiceDetails.IfUnresolvedReturnDefaultIfNotRegistered))
          : new WithDetails(serviceType,
          ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, null, metadataKey, metadata));
    }

    /// <summary>Creates service info using typed <typeparamref name="TService"/>.</summary>
    public static Typed<TService> Of<TService>(IfUnresolved ifUnresolved = IfUnresolved.Throw, object serviceKey = null) =>
        serviceKey == null && ifUnresolved == IfUnresolved.Throw
            ? new Typed<TService>()
            : new TypedWithDetails<TService>(ServiceDetails.Of(null, serviceKey, ifUnresolved));

    /// <summary>Strongly-typed version of Service Info.</summary> <typeparam name="TService">Service type.</typeparam>
    public class Typed<TService> : ServiceInfo
    {
      /// <summary>Creates service info object.</summary>
      public Typed() : base(typeof(TService)) { }
    }

    /// <summary>Type of service to create. Indicates registered service in registry.</summary>
    public Type ServiceType { get; }

    /// <summary>Shortcut access to service key</summary>
    public object ServiceKey => Details.ServiceKey;

    /// <summary>Additional settings. If not specified uses <see cref="ServiceDetails.Default"/>.</summary>
    public virtual ServiceDetails Details => ServiceDetails.Default;

    /// <summary>Creates info from service type and details.</summary>
    public IServiceInfo Create(Type serviceType, ServiceDetails details) =>
        details == ServiceDetails.Default ? new ServiceInfo(serviceType) : new WithDetails(serviceType, details);

    /// <summary>Prints info to string using <see cref="ServiceInfoTools.Print"/>.</summary> <returns>Printed string.</returns>
    public override string ToString() =>
        new StringBuilder().Print(this).ToString();

    #region Implementation

    private ServiceInfo(Type serviceType) { ServiceType = serviceType; }

    private class WithDetails : ServiceInfo
    {
      public override ServiceDetails Details => _details;
      public WithDetails(Type serviceType, ServiceDetails details) : base(serviceType) { _details = details; }
      private readonly ServiceDetails _details;
    }

    private class TypedWithDetails<TService> : Typed<TService>
    {
      public override ServiceDetails Details => _details;
      public TypedWithDetails(ServiceDetails details) { _details = details; }
      private readonly ServiceDetails _details;
    }

    #endregion
  }

  /// <summary>Provides <see cref="IServiceInfo"/> for parameter,
  /// by default using parameter name as <see cref="IServiceInfo.ServiceType"/>.</summary>
  /// <remarks>For parameter default setting <see cref="ServiceDetails.IfUnresolved"/> is <see cref="IfUnresolved.Throw"/>.</remarks>
  public class ParameterServiceInfo : IServiceInfo
  {
    /// <summary>Creates service info from parameter alone, setting service type to parameter type,
    /// and setting resolution policy to <see cref="IfUnresolved.ReturnDefault"/> if parameter is optional.</summary>
    public static ParameterServiceInfo Of(ParameterInfo parameter)
    {
      if (!parameter.IsOptional)
        return new ParameterServiceInfo(parameter);
      return new WithDetails(parameter, parameter.DefaultValue == null
          ? ServiceDetails.IfUnresolvedReturnDefault
          : ServiceDetails.Of(ifUnresolved: IfUnresolved.ReturnDefault, defaultValue: parameter.DefaultValue));
    }

    /// <summary>The parameter type or dereferenced parameter type for `ref`, `in`, `out` parameters</summary>
    public readonly Type DereferencedParameterType;

    /// <summary>Service type specified by <see cref="ParameterInfo.ParameterType"/>.</summary>
    public virtual Type ServiceType => DereferencedParameterType;

    /// <summary>Optional service details.</summary>
    public virtual ServiceDetails Details => ServiceDetails.Default;

    /// <summary>Creates info from service type and details.</summary>
    public IServiceInfo Create(Type serviceType, ServiceDetails details) =>
        serviceType == ServiceType ? new WithDetails(Parameter, details) : new TypeWithDetails(Parameter, serviceType, details);

    /// <summary>Parameter info.</summary>
    public readonly ParameterInfo Parameter;

    /// <summary>Prints info to string using <see cref="ServiceInfoTools.Print"/>.</summary> <returns>Printed string.</returns>
    public override string ToString() =>
        new StringBuilder().Print(this).Append(" as parameter ").Print(Parameter.Name).ToString();

    private ParameterServiceInfo(ParameterInfo p)
    {
      Parameter = p;
      DereferencedParameterType = p.ParameterType.IsByRef
          ? p.ParameterType.GetElementType()
          : p.ParameterType;
    }

    private class WithDetails : ParameterServiceInfo
    {
      public override ServiceDetails Details { get { return _details; } }
      public WithDetails(ParameterInfo parameter, ServiceDetails details) : base(parameter) => _details = details;
      private readonly ServiceDetails _details;
    }

    private sealed class TypeWithDetails : WithDetails
    {
      public override Type ServiceType { get { return _serviceType; } }
      public TypeWithDetails(ParameterInfo parameter, Type serviceType, ServiceDetails details) : base(parameter, details) => _serviceType = serviceType;
      private readonly Type _serviceType;
    }
  }

  /// <summary>Base class for property and field dependency info.</summary>
  public abstract class PropertyOrFieldServiceInfo : IServiceInfo
  {
    /// <summary>Create member info out of provide property or field.</summary>
    /// <param name="member">Member is either property or field.</param> <returns>Created info.</returns>
    public static PropertyOrFieldServiceInfo Of(MemberInfo member) =>
        member.ThrowIfNull() is PropertyInfo
            ? (PropertyOrFieldServiceInfo)new Property((PropertyInfo)member)
            : new Field((FieldInfo)member);

    /// <summary>The required service type. It will be either <see cref="FieldInfo.FieldType"/> or <see cref="PropertyInfo.PropertyType"/>.</summary>
    public abstract Type ServiceType { get; }

    /// <summary>Optional details: service key, if-unresolved policy, required service type.</summary>
    public virtual ServiceDetails Details => ServiceDetails.IfUnresolvedReturnDefaultIfNotRegistered;

    /// <summary>Creates info from service type and details.</summary>
    /// <param name="serviceType">Required service type.</param> <param name="details">Optional details.</param> <returns>Create info.</returns>
    public abstract IServiceInfo Create(Type serviceType, ServiceDetails details);

    /// <summary>Either <see cref="PropertyInfo"/> or <see cref="FieldInfo"/>.</summary>
    public abstract MemberInfo Member { get; }

    /// <summary>Sets property or field value on provided holder object.</summary>
    /// <param name="holder">Holder of property or field.</param> <param name="value">Value to set.</param>
    public abstract void SetValue(object holder, object value);

    #region Implementation

    private class Property : PropertyOrFieldServiceInfo
    {
      public override Type ServiceType => _property.PropertyType;

      public override IServiceInfo Create(Type serviceType, ServiceDetails details) =>
          serviceType == ServiceType ? new WithDetails(_property, details) : new TypeWithDetails(_property, serviceType, details);

      public override MemberInfo Member => _property;

      public override void SetValue(object holder, object value) => _property.SetValue(holder, value, null);

      public override string ToString() =>
          new StringBuilder().Print(this).Append(" as property ").Print(_property.Name).ToString();

      private readonly PropertyInfo _property;
      public Property(PropertyInfo property) { _property = property; }

      private class WithDetails : Property
      {
        public override ServiceDetails Details { get; }

        public WithDetails(PropertyInfo property, ServiceDetails details) : base(property) { Details = details; }
      }

      private sealed class TypeWithDetails : WithDetails
      {
        public override Type ServiceType { get; }

        public TypeWithDetails(PropertyInfo property, Type serviceType, ServiceDetails details)
            : base(property, details) { ServiceType = serviceType; }
      }
    }

    private class Field : PropertyOrFieldServiceInfo
    {
      public override Type ServiceType => _field.FieldType;

      public override IServiceInfo Create(Type serviceType, ServiceDetails details) =>
          serviceType == null ? new WithDetails(_field, details) : new TypeWithDetails(_field, serviceType, details);

      public override MemberInfo Member => _field;

      public override void SetValue(object holder, object value) => _field.SetValue(holder, value);

      public override string ToString() =>
          new StringBuilder().Print(this).Append(" as field ").Print(_field.Name).ToString();

      private readonly FieldInfo _field;
      public Field(FieldInfo field) { _field = field; }

      private class WithDetails : Field
      {
        public override ServiceDetails Details { get; }

        public WithDetails(FieldInfo field, ServiceDetails details) : base(field) { Details = details; }
      }

      private sealed class TypeWithDetails : WithDetails
      {
        public override Type ServiceType { get; }

        public TypeWithDetails(FieldInfo field, Type serviceType, ServiceDetails details) : base(field, details)
        { ServiceType = serviceType; }
      }
    }

    #endregion
  }

  /// <summary>Stored check results of two kinds: inherited down dependency chain and not.</summary>
  [Flags]
  public enum RequestFlags
  {
    /// <summary>Not inherited</summary>
    TracksTransientDisposable = 1 << 1,

    /// <summary>Inherited</summary>
    IsSingletonOrDependencyOfSingleton = 1 << 3,

    /// <summary>Inherited</summary>
    IsWrappedInFunc = 1 << 4,

    /// <summary>Indicates that the request is the one from Resolve call.</summary>
    IsResolutionCall = 1 << 5,

    /// <summary>Non inherited</summary>
    OpensResolutionScope = 1 << 6,

    /// <summary>Non inherited</summary>
    StopRecursiveDependencyCheck = 1 << 7,

    /// <summary>Non inherited. Marks the expression to be added to generated resolutions to prevent infinite recursion</summary>
    IsGeneratedResolutionDependencyExpression = 1 << 8,

    /// <summary>Non inherited. Indicates the root service inside the function.</summary>
    IsDirectlyWrappedInFunc = 1 << 9,
  }

  /// Helper extension methods to use on the bunch of factories instead of lambdas to minimize allocations
  internal static class RequestTools
  {
    public static bool MatchFactoryConditionAndMetadata(this Request request, Factory factory)
    {
      if (!factory.CheckCondition(request))
        return false;

      var metadataKey = request.MetadataKey;
      var metadata = request.Metadata;
      return (metadataKey == null && metadata == null) || factory.Setup.MatchesMetadata(metadataKey, metadata);
    }
    public static bool MatchFactoryReuse(this Request r, Factory f) => f.Reuse?.CanApply(r) ?? true;
  }

  internal sealed class RequestStack
  {
    public static RequestStack Get(int index = 0)
    {
      var capacity = 4;
      while (index >= capacity)
        capacity <<= 1;
      return new RequestStack(capacity);
    }

    public static RequestStack Create(int capacityPowerOfTwo) =>
        new RequestStack(capacityPowerOfTwo);

    public Request[] Items;
    private RequestStack(int capacity) => Items = new Request[capacity];

    public ref Request GetOrPushRef(int index)
    {
      if (index < Items.Length)
        return ref Items[index];

      Items = Expand(Items, index);
      return ref Items[index];
    }

    private static Request[] Expand(Request[] items, int index)
    {
      var count = items.Length;
      var newCount = count << 1;

      // ensure that the index is always in range
      while (index >= newCount)
        newCount <<= 1;

      var newItems = new Request[newCount]; // count x 2
      Array.Copy(items, 0, newItems, 0, count);
      return newItems;
    }
  }

  /// <summary>Tracks the requested service and resolved factory details in a chain of nested dependencies.</summary>
  public sealed class Request : IEnumerable<Request>
  {
    internal static readonly RequestFlags InheritedFlags
        = RequestFlags.IsSingletonOrDependencyOfSingleton
        | RequestFlags.IsWrappedInFunc;

    private const RequestFlags DefaultFlags = default;

    /// <summary>Empty terminal request.</summary>
    public static readonly Request Empty =
        new Request(null, null, 0, 0, null, DefaultFlags, ServiceInfo.Empty, null);

    internal static readonly Expression EmptyRequestExpr =
        Field(null, typeof(Request).Field(nameof(Empty)));

    /// <summary>Empty request which opens resolution scope.</summary>
    public static readonly Request EmptyOpensResolutionScope =
        new Request(null, null, 0, 0, null, DefaultFlags | RequestFlags.OpensResolutionScope | RequestFlags.IsResolutionCall,
            ServiceInfo.Empty, null);

    internal static readonly Expression EmptyOpensResolutionScopeRequestExpr =
        Field(null, typeof(Request).Field(nameof(EmptyOpensResolutionScope)));

    internal static Request CreateForValidation(IContainer container, IServiceInfo serviceInfo, RequestStack stack)
    {
      // we are re-starting the dependency depth count from `1`
      ref var req = ref stack.GetOrPushRef(0);
      if (req == null)
        req = new Request(container, Empty, 1, 0, stack, DefaultFlags | RequestFlags.IsResolutionCall, serviceInfo, null);
      else
        req.SetServiceInfo(container, Empty, 1, 0, stack, DefaultFlags | RequestFlags.IsResolutionCall, serviceInfo, null);
      return req;
    }

    /// <summary>Creates the Resolve request. The container initiated the Resolve is stored within request.</summary>
    public static Request Create(IContainer container, IServiceInfo serviceInfo,
        Request preResolveParent = null, RequestFlags flags = DefaultFlags, object[] inputArgs = null)
    {
      var serviceType = serviceInfo.ServiceType;
      if (serviceType != null && serviceType.IsOpenGeneric())
        Throw.It(Error.ResolvingOpenGenericServiceTypeIsNotPossible, serviceType);

      flags |= RequestFlags.IsResolutionCall;

      // inherit some flags and service details from parent (if any)
      preResolveParent = preResolveParent ?? Empty;
      if (!preResolveParent.IsEmpty)
      {
        serviceInfo = serviceInfo.InheritInfoFromDependencyOwner(
            preResolveParent._serviceInfo, container, preResolveParent.FactoryType);

        flags |= preResolveParent.Flags & InheritedFlags;
      }

      var inputArgExprs = inputArgs?.Map(a => Constant(a)); // todo: @check what happens if `a == null`, does the `object` type for is fine

      var stack = RequestStack.Get();
      ref var req = ref stack.GetOrPushRef(0);

      // we are re-starting the dependency depth count from `1`
      if (req == null)
        req = new Request(container, preResolveParent, 1, 0, stack, flags, serviceInfo, inputArgExprs);
      else
        req.SetServiceInfo(container, preResolveParent, 1, 0, stack, flags, serviceInfo, inputArgExprs);
      return req;
    }

    /// <summary>Creates the Resolve request. The container initiated the Resolve is stored within request.</summary>
    public static Request Create(IContainer container, Type serviceType,
        object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
        Request preResolveParent = null, RequestFlags flags = DefaultFlags, object[] inputArgs = null) =>
        Create(container, ServiceInfo.Of(serviceType, requiredServiceType, ifUnresolved, serviceKey),
            preResolveParent, flags, inputArgs);

    // todo: Make a property in v5.0
    /// <summary>Available in runtime only, provides access to container initiated the request.</summary>
    public IContainer Container;

    /// <summary>Request immediate parent.</summary>
    public Request DirectParent;

    internal RequestStack RequestStack;
    internal int IndexInStack => DependencyDepth - 1;

    // mutable because of RequestFlags.AddedToResolutionExpressions
    /// <summary>Persisted request conditions</summary>
    public RequestFlags Flags;

    /// mutable, so that the ServiceKey or IfUnresolved can be changed in place.
    internal IServiceInfo _serviceInfo;

    /// <summary>Input arguments provided with `Resolve`</summary>
    internal Expression[] InputArgExprs;

    /// <summary>Runtime known resolve factory, otherwise is <c>null</c></summary>
    internal Factory Factory;

    /// <summary>Resolved factory ID, used to identify applied decorator.</summary>
    public int FactoryID { get; private set; }

    // based on FactoryID
    private int _hashCode;

    /// <summary>Type of factory: Service, Wrapper, or Decorator.</summary>
    public FactoryType FactoryType { get; private set; }

    /// <summary>Combines decorator and <see cref="DecoratedFactoryID"/></summary>
    public int CombineDecoratorWithDecoratedFactoryID() =>
        FactoryID | (DecoratedFactoryID << 16);

    /// <summary>Service implementation type if known.</summary>
    public Type ImplementationType => _factoryImplType ?? Factory?.ImplementationType;
    private Type _factoryImplType;

    /// <summary>Service reuse.</summary>
    public IReuse Reuse { get; private set; }

    /// <summary>ID of decorated factory in case of decorator factory type</summary>
    public int DecoratedFactoryID { get; private set; }

    /// <summary>Number of nested dependencies. Set with each new Push.</summary>
    public int DependencyDepth;

    /// <summary>The total dependency count</summary>
    public int DependencyCount;

    internal void DecreaseTrackedDependencyCountForParents(int dependencyCount)
    {
      for (var p = DirectParent; !p.IsEmpty; p = p.DirectParent)
        p.DependencyCount -= dependencyCount;
    }

    /// <summary>Indicates that request is empty initial request.</summary>
    public bool IsEmpty => DirectParent == null;

    /// <summary>Returns true if request is First in First Resolve call.</summary>
    public bool IsResolutionRoot => !IsEmpty && DirectParent.IsEmpty;

    /// <summary>Returns true if request is First in Resolve call.</summary>
    public bool IsResolutionCall => !IsEmpty && (Flags & RequestFlags.IsResolutionCall) != 0;

    /// <summary>Not the root resolution call.</summary>
    public bool IsNestedResolutionCall => IsResolutionCall && !DirectParent.IsEmpty;

    /// <summary>Returns true if request is First in First Resolve call.</summary>
    public bool OpensResolutionScope => !IsEmpty && (DirectParent.Flags & RequestFlags.OpensResolutionScope) != 0;

    /// <summary>Checks if the request Or its parent is wrapped in Func.
    /// Use <see cref="IsDirectlyWrappedInFunc"/> for the direct Func wrapper.</summary>
    public bool IsWrappedInFunc() => (Flags & RequestFlags.IsWrappedInFunc) != 0;

    /// <summary>Checks if the request is directly wrapped in Func</summary>
    public bool IsDirectlyWrappedInFunc() => (Flags & RequestFlags.IsDirectlyWrappedInFunc) != 0;

    /// <summary>Checks if request has parent with service type of Func with arguments.</summary>
    public bool IsWrappedInFuncWithArgs() => InputArgExprs != null;

    /// <summary>Returns expression for func arguments.</summary>
    public Expression GetInputArgsExpr() =>
        InputArgExprs == null ? Constant(null, typeof(object[]))
        : (Expression)NewArrayInit(typeof(object), InputArgExprs.Map(x => x.Type.IsValueType() ? Convert(x, typeof(object)) : x));

    /// <summary>Indicates that requested service is transient disposable that should be tracked.</summary>
    public bool TracksTransientDisposable => (Flags & RequestFlags.TracksTransientDisposable) != 0;

    /// <summary>Indicates the request is singleton or has singleton upper in dependency chain.</summary>
    public bool IsSingletonOrDependencyOfSingleton => (Flags & RequestFlags.IsSingletonOrDependencyOfSingleton) != 0;

    /// <summary>Is not used</summary>
    [Obsolete("Is not used - hides more than abstracts")]
    public bool ShouldSplitObjectGraph() =>
        FactoryType == FactoryType.Service &&
        DependencyDepth > Rules.DependencyDepthToSplitObjectGraph;

    /// <summary>Current scope</summary>
    public IScope CurrentScope => Container.CurrentScope;

    /// <summary>Singletons</summary>
    public IScope SingletonScope => Container.SingletonScope;

    /// <summary>Shortcut to issued container rules.</summary>
    public Rules Rules => Container.Rules;

    /// <summary>(optional) Made spec used for resolving request.</summary>
    public Made Made => Factory?.Made;

    /// <summary>Returns service parent skipping wrapper if any. To get direct parent use <see cref="DirectParent"/>.</summary>
    public Request Parent
    {
      get
      {
        var p = DirectParent;
        if (p != null)
          while (p.DirectParent != null && p.FactoryType == FactoryType.Wrapper)
            p = p.DirectParent;
        return p;
      }
    }

    /// <summary>Requested service type.</summary>
    public Type ServiceType => _serviceInfo.ServiceType;

    /// <summary>Compatible required or service type.</summary>
    public Type GetActualServiceType() => _actualServiceType;
    private Type _actualServiceType;

    /// <summary>Optional service key to identify service of the same type.</summary>
    public object ServiceKey => _serviceInfo.Details.ServiceKey;

    /// <summary>Metadata key to find in metadata dictionary in resolved service.</summary>
    public string MetadataKey => _serviceInfo.Details.MetadataKey;

    /// <summary>Metadata or the value (if key specified) to find in resolved service.</summary>
    public object Metadata => _serviceInfo.Details.Metadata;

    /// <summary>Policy to deal with unresolved service.</summary>
    public IfUnresolved IfUnresolved => _serviceInfo.Details.IfUnresolved;

    /// <summary>Required service type if specified.</summary>
    public Type RequiredServiceType => _serviceInfo.Details.RequiredServiceType;

    /// <summary>Relative number representing reuse lifespan.</summary>
    public int ReuseLifespan => Reuse?.Lifespan ?? 0;

    /// <summary>Known implementation, or otherwise actual service type.</summary>
    public Type GetKnownImplementationOrServiceType() => _factoryImplType ?? Factory?.ImplementationType ?? _actualServiceType;

    /// <summary>Creates new request with provided info, and links current request as a parent.
    /// Allows to set some additional flags. Existing/parent request should be resolved to 
    /// factory via `WithResolvedFactory` before pushing info into it.</summary>
    public Request Push(IServiceInfo info, RequestFlags additionalFlags = DefaultFlags)
    {
      if (FactoryID == 0)
        Throw.It(Error.PushingToRequestWithoutFactory, info, this);

      var flags = Flags & InheritedFlags | additionalFlags;
      var serviceInfo = info.ThrowIfNull().InheritInfoFromDependencyOwner(_serviceInfo, Container, FactoryType);

      var stack = RequestStack;
      var indexInStack = IndexInStack + 1;
      if (stack == null)
      {
        stack = RequestStack.Get(indexInStack);

        // traverse all the requests up including the resolution root and set the new stack to them
        Request parent = null;
        do
        {
          parent = parent == null ? this : parent.DirectParent;
          parent.RequestStack = stack;
        }
        while ((parent.Flags & RequestFlags.IsResolutionCall) == 0 && !parent.DirectParent.IsEmpty);
      }

      ref var req = ref stack.GetOrPushRef(indexInStack);
      if (req == null)
        req = new Request(Container, this, DependencyDepth + 1, 0,
            RequestStack, flags, serviceInfo, InputArgExprs);
      else
        req.SetServiceInfo(Container, this, DependencyDepth + 1, 0,
            RequestStack, flags, serviceInfo, InputArgExprs);

      return req;
    }

    /// <summary>Composes service description into <see cref="IServiceInfo"/> and Pushes the new request.</summary>
    public Request Push(Type serviceType, object serviceKey = null,
        IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null, RequestFlags flags = DefaultFlags) =>
        Push(ServiceInfo.Of(serviceType.ThrowIfNull().ThrowIf(serviceType.IsOpenGeneric(), Error.ResolvingOpenGenericServiceTypeIsNotPossible),
            requiredServiceType, ifUnresolved, serviceKey), flags);

    #region Used in generated expression

    /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
    public Request Push(Type serviceType, int factoryID, Type implementationType, IReuse reuse) =>
        Push(serviceType, null, null, null, null, IfUnresolved.Throw,
            factoryID, FactoryType.Service, implementationType, reuse, DefaultFlags, 0);

    internal static readonly Lazy<MethodInfo> PushMethodWith4Args = Lazy.Of(() =>
        typeof(Request).Method(nameof(Push), typeof(Type), typeof(int), typeof(Type), typeof(IReuse)));

    /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
    public Request Push(Type serviceType, Type requiredServiceType, object serviceKey,
        int factoryID, FactoryType factoryType, Type implementationType, IReuse reuse, RequestFlags flags) =>
        Push(serviceType, requiredServiceType, serviceKey, null, null, IfUnresolved.Throw,
            factoryID, factoryType, implementationType, reuse, flags, 0);

    internal static readonly Lazy<MethodInfo> PushMethodWith8Args = Lazy.Of(() =>
        typeof(Request).Method(nameof(Push), typeof(Type), typeof(Type), typeof(object),
            typeof(int), typeof(FactoryType), typeof(Type), typeof(IReuse), typeof(RequestFlags)));

    /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
    public Request Push(Type serviceType, Type requiredServiceType, object serviceKey, IfUnresolved ifUnresolved,
        int factoryID, FactoryType factoryType, Type implementationType, IReuse reuse, RequestFlags flags,
        int decoratedFactoryID) =>
        Push(serviceType, requiredServiceType, serviceKey, null, null, ifUnresolved,
            factoryID, factoryType, implementationType, reuse, flags, decoratedFactoryID);

    internal static readonly Lazy<MethodInfo> PushMethodWith10Args = Lazy.Of(() =>
        typeof(Request).Method(nameof(Push),
            typeof(Type), typeof(Type), typeof(object), typeof(IfUnresolved),
            typeof(int), typeof(FactoryType), typeof(Type), typeof(IReuse), typeof(RequestFlags), typeof(int)));

    /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
    public Request Push(
        Type serviceType, Type requiredServiceType, object serviceKey, string metadataKey, object metadata, IfUnresolved ifUnresolved,
        int factoryID, FactoryType factoryType, Type implementationType, IReuse reuse, RequestFlags flags, int decoratedFactoryID)
    {
      return new Request(Container, this, DependencyDepth + 1, 0, null, flags,
          ServiceInfo.Of(serviceType, requiredServiceType, ifUnresolved, serviceKey, metadataKey, metadata),
          InputArgExprs, implementationType, null, // factory cannot be supplied in generated code
          factoryID, factoryType, reuse, decoratedFactoryID);
    }

    internal static readonly Lazy<MethodInfo> PushMethodWith12Args = Lazy.Of(() =>
        typeof(Request).Method(nameof(Push),
        typeof(Type), typeof(Type), typeof(object), typeof(string), typeof(object), typeof(IfUnresolved),
        typeof(int), typeof(FactoryType), typeof(Type), typeof(IReuse), typeof(RequestFlags), typeof(int)));

    #endregion

    /// <summary>Allow to switch current service info to the new one, e.g. in decorators.
    /// If info did not change then return the same this request.</summary>
    public Request WithChangedServiceInfo(Func<IServiceInfo, IServiceInfo> getInfo)
    {
      var newServiceInfo = getInfo(_serviceInfo);
      return newServiceInfo == _serviceInfo ? this
          : new Request(Container, DirectParent, DependencyDepth, DependencyCount,
              RequestStack, Flags, newServiceInfo, InputArgExprs,
              _factoryImplType, Factory, FactoryID, FactoryType, Reuse, DecoratedFactoryID);
    }

    /// Produces the new request with the changed `ifUnresolved` or returns original request otherwise
    public Request WithIfUnresolved(IfUnresolved ifUnresolved) =>
        IfUnresolved == ifUnresolved ? this
            : new Request(Container, DirectParent, DependencyDepth, DependencyCount,
                RequestStack, Flags, _serviceInfo.WithIfUnresolved(ifUnresolved), InputArgExprs,
                _factoryImplType, Factory, FactoryID, FactoryType, Reuse, DecoratedFactoryID);

    // todo: in place mutation?
    /// <summary>Updates the flags</summary>
    public Request WithFlags(RequestFlags newFlags) =>
        new Request(Container, DirectParent, DependencyDepth, DependencyCount,
            RequestStack, newFlags, _serviceInfo, InputArgExprs,
            _factoryImplType, Factory, FactoryID, FactoryType, Reuse, DecoratedFactoryID);

    // note: Mutates the request, required for proper caching
    /// <summary>Sets service key to passed value. Required for multiple default services to change null key to
    /// actual <see cref="DefaultKey"/></summary>
    public void ChangeServiceKey(object serviceKey)
    {
      var info = _serviceInfo;
      var details = info.Details;
      _serviceInfo = info.Create(info.ServiceType,
          ServiceDetails.Of(details.RequiredServiceType, serviceKey, details.IfUnresolved, details.DefaultValue));
    }

    /// <summary>Prepends input arguments to existing arguments in request. It is done because the
    /// nested Func/Action input argument has a priority over outer argument.
    /// The arguments are provided by Func and Action wrappers, or by `args` parameter in Resolve call.</summary>
    public Request WithInputArgs(Expression[] inputArgs) =>
        new Request(Container, DirectParent, DependencyDepth, DependencyCount,
            RequestStack, Flags, _serviceInfo, inputArgs.Append(InputArgExprs),
            _factoryImplType, Factory, FactoryID, FactoryType, Reuse, DecoratedFactoryID);

    /// <summary>Returns new request with set implementation details.</summary>
    /// <param name="factory">Factory to which request is resolved.</param>
    /// <param name="skipRecursiveDependencyCheck">(optional) does not check for recursive dependency.
    /// Use with caution. Make sense for Resolution expression.</param>
    /// <param name="skipCaptiveDependencyCheck">(optional) allows to skip reuse mismatch aka captive dependency check.</param>
    /// <param name="copyRequest">Make a defensive copy of request.</param>
    /// <returns>New request with set factory.</returns>
    public Request WithResolvedFactory(Factory factory,
        bool skipRecursiveDependencyCheck = false, bool skipCaptiveDependencyCheck = false, bool copyRequest = false)
    {
      var factoryId = factory.FactoryID;
      var decoratedFactoryID = 0;
      if (Factory != null) // resolving the factory for the second time, usually happens in decorators
      {
        if (Factory.FactoryID == factoryId)
          return this; // stop resolving to the same factory twice

        if (Factory.FactoryType != FactoryType.Decorator &&
            factory.FactoryType == FactoryType.Decorator)
          decoratedFactoryID = FactoryID;
      }

      // it is required to nullify the TD tracking when factory is resolved multiple times, e.g. for decorator
      var flags = Flags & ~RequestFlags.TracksTransientDisposable;
      if (skipRecursiveDependencyCheck)
        flags |= RequestFlags.StopRecursiveDependencyCheck;

      var setup = factory.Setup;

      IReuse reuse = null;
      if (InputArgExprs != null && Rules.IgnoringReuseForFuncWithArgs)
        reuse = Crystal.Reuse.Transient;
      else if (factory.Reuse != null)
        reuse = factory.Reuse;
      else if (setup.UseParentReuse)
        reuse = DirectParent.IsEmpty ? Rules.DefaultReuse : null; // the `null` here signals to find the parent reuse
      else if (factory.FactoryType == FactoryType.Decorator &&
               (setup.To<Setup.DecoratorSetup>().UseDecorateeReuse || Rules.UseDecorateeReuseForDecorators))
        reuse = Reuse; // already resolved decoratee reuse
      else if (factory.FactoryType == FactoryType.Wrapper)
        reuse = Crystal.Reuse.Transient;
      else
        reuse = Rules.DefaultReuse;

      IReuse firstParentNonTransientReuseOrNull = null;
      if (!DirectParent.IsEmpty)
      {
        var checkRecursiveDependency = !skipRecursiveDependencyCheck &&
            factory.FactoryType == FactoryType.Service;

        var reuseLifespan = reuse?.Lifespan ?? 0;

        var checkCaptiveDependency = !skipCaptiveDependencyCheck && !IsDirectlyWrappedInFunc() && !factory.Setup.OpenResolutionScope &&
            (Rules.ThrowIfDependencyHasShorterReuseLifespan && reuseLifespan > 0 ||
             Rules.ThrowIfScopedOrSingletonHasTransientDependency && reuseLifespan == 0);

        var scopedOrSingleton = checkCaptiveDependency &&
            (reuse as CurrentScopeReuse)?.ScopedOrSingleton == true;

        // Means we are incrementing the count when resolving the Factory for the first time,
        // and not twice for the decorators
        var dependencyCountIncrement = Factory == null ? 1 : 0;

        for (var p = DirectParent; !p.IsEmpty; p = p.DirectParent)
        {
          if (checkRecursiveDependency)
          {
            if ((p.Flags & RequestFlags.StopRecursiveDependencyCheck) != 0)
              checkRecursiveDependency = false; // stop the check
            else if (p.FactoryID == factoryId)
              Throw.It(Error.RecursiveDependencyDetected, Print(factoryId));
          }

          if (checkCaptiveDependency)
          {
            if (p.OpensResolutionScope ||
                scopedOrSingleton && p.Reuse is SingletonReuse ||
                p.FactoryType == FactoryType.Wrapper && p._actualServiceType.IsFunc())
              checkCaptiveDependency = false; // stop the check
            else if (p.FactoryType == FactoryType.Service && p.ReuseLifespan > reuseLifespan)
              Throw.It(Error.DependencyHasShorterReuseLifespan, PrintCurrent(), reuse, p);
          }

          if (firstParentNonTransientReuseOrNull == null)
          {
            if (p.FactoryType != FactoryType.Wrapper)
            {
              if (p.Reuse != Crystal.Reuse.Transient)
                firstParentNonTransientReuseOrNull = p.Reuse;
            }
            else if (p._actualServiceType.IsFunc())
              firstParentNonTransientReuseOrNull = Rules.DefaultReuse;
          }

          p.DependencyCount += dependencyCountIncrement;
        }

        if (reuse == null) // for the `setup.UseParentReuse`
          reuse = firstParentNonTransientReuseOrNull ?? Rules.DefaultReuse;
      }

      if (reuse == Crystal.Reuse.Singleton)
      {
        flags |= RequestFlags.IsSingletonOrDependencyOfSingleton;
      }
      else if (reuse == Crystal.Reuse.Transient)
      {
        // check for disposable transient
        if (!setup.PreventDisposal &&
            (setup.TrackDisposableTransient || !setup.AllowDisposableTransient && Rules.TrackingDisposableTransients) &&
            typeof(IDisposable).GetTypeInfo().IsAssignableFrom((factory.ImplementationType ?? _actualServiceType).GetTypeInfo()))
        {
          if (firstParentNonTransientReuseOrNull != null)
          {
            reuse = firstParentNonTransientReuseOrNull;
            flags |= RequestFlags.TracksTransientDisposable;
          }
          else if (!IsWrappedInFunc())
          {
            reuse = Crystal.Reuse.ScopedOrSingleton;
            flags |= RequestFlags.TracksTransientDisposable;
          }
        }
      }

      if (copyRequest)
      {
        IsolateRequestChain();
        return new Request(Container, DirectParent, DependencyDepth, DependencyCount, null, flags,
            _serviceInfo, InputArgExprs, null, factory, factoryId, factory.FactoryType, reuse, decoratedFactoryID);
      }

      Flags = flags;
      SetResolvedFactory(null, factory, factoryId, factory.FactoryType, reuse, decoratedFactoryID);
      return this;
    }

    /// <summary>Check for the parents.</summary>
    public bool HasRecursiveParent(int factoryID)
    {
      for (var p = DirectParent; !p.IsEmpty; p = p.DirectParent)
      {
        if ((p.Flags & RequestFlags.StopRecursiveDependencyCheck) != 0)
          break; // stops further upward checking
        if (p.FactoryID == factoryID)
          return true;
      }
      return false;
    }

    /// <summary>If request corresponds to dependency injected into parameter,
    /// then method calls <paramref name="parameter"/> handling and returns its result.
    /// If request corresponds to property or field, then method calls respective handler.
    /// If request does not correspond to dependency, then calls <paramref name="root"/> handler.</summary>
    public TResult Is<TResult>(
        Func<TResult> root = null,
        Func<ParameterInfo, TResult> parameter = null,
        Func<PropertyInfo, TResult> property = null,
        Func<FieldInfo, TResult> field = null)
    {
      var info = _serviceInfo;
      if (info is ParameterServiceInfo par)
      {
        if (parameter != null)
          return parameter(par.Parameter);
      }
      else if (info is PropertyOrFieldServiceInfo propOrField)
      {
        if (propOrField.Member is PropertyInfo propertyInfo)
        {
          if (property != null)
            return property(propertyInfo);
        }
        else if (field != null)
          return field((FieldInfo)propOrField.Member);
      }
      else if (root != null)
        return root();

      return default(TResult);
    }

    /// <summary>Obsolete: now request is directly implements the <see cref="IEnumerable{T}"/>.</summary>
    public IEnumerable<Request> Enumerate() => this;

    /// <summary>Enumerates self and all request stack parents.</summary>
    public IEnumerator<Request> GetEnumerator()
    {
      for (var r = this; !r.IsEmpty; r = r.DirectParent)
        yield return r;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Prints current request info only (no parents printed) to provided builder.</summary>
    public StringBuilder PrintCurrent(StringBuilder s = null)
    {
      s = s ?? new StringBuilder();

      if (IsEmpty)
        return s.Append("<empty request>");

      var isResolutionCall = false;
      if (isResolutionCall = IsNestedResolutionCall)
        s.Append("resolution call dependency ");
      else if (isResolutionCall = IsResolutionRoot)
        s.Append("resolution root ");

      if (FactoryID != 0) // request is with resolved factory
      {
        if (Reuse != Crystal.Reuse.Transient)
          s.Append(Reuse is SingletonReuse ? "Singleton" : "Scoped").Append(' ');

        if (FactoryType != FactoryType.Service)
          s.Append(FactoryType.ToString().ToLower()).Append(' ');

        var implType = ImplementationType;
        if (implType != null && implType != ServiceType)
          s.Print(implType).Append(": ");
      }

      s.Append(_serviceInfo);

      if (Factory != null && Factory is ReflectionFactory == false)
        s.Append(' ').Append(Factory.GetType().Name).Append(' ');

      if (FactoryID != 0)
        s.Append(" FactoryId=").Append(FactoryID);

      if (DecoratedFactoryID != 0)
        s.Append(" decorating FactoryId=").Append(DecoratedFactoryID);

      if (!InputArgExprs.IsNullOrEmpty())
        s.AppendFormat(" with passed arguments [{0}]", InputArgExprs);

      var flags = Flags;
      if (isResolutionCall) // excluding the doubled info
        flags &= ~RequestFlags.IsResolutionCall;

      if (flags != default(RequestFlags))
        s.Append(" (").Append(Flags).Append(')');

      return s;
    }

    /// <summary>Prints full stack of requests starting from current one using <see cref="PrintCurrent"/>.</summary>
    public StringBuilder Print(int recursiveFactoryID = 0)
    {
      if (IsEmpty)
        return new StringBuilder("<empty request>");

      var s = PrintCurrent(new StringBuilder());

      s = recursiveFactoryID == 0 ? s : s.Append(" <--recursive");
      foreach (var parent in DirectParent)
      {
        s = parent.PrintCurrent(s.AppendLine().Append("  in "));
        if (parent.FactoryID == recursiveFactoryID)
          s = s.Append(" <--recursive");
      }

      if (Container != null)
        s.AppendLine().Append("  from ").Append(Container);

      return s;
    }

    /// <summary>Prints whole request chain.</summary>
    public override string ToString() => Print().ToString();

    /// <summary>Returns true if request info and passed object are equal, and their parents recursively are equal.</summary>
    public override bool Equals(object obj) => Equals(obj as Request);

    /// <summary>Returns true if request info and passed info are equal, and their parents recursively are equal.</summary>
    public bool Equals(Request other) =>
        other != null && EqualsWithoutParent(other)
            && (DirectParent == null && other.DirectParent == null
            || (DirectParent != null && DirectParent.EqualsWithoutParent(other.DirectParent)));

    // todo: Seems like Equals and GetHashCode are not used anymore - can we remove them
    // todo: Should we include InputArgs and DecoratedFactoryID and what about flags? 
    // todo: Should we add and rely on Equals of ServiceInfo and Reuse?
    // todo: The equals calculated differently comparing to HashCode, may be we can use FactoryID for Equals as well?
    /// <summary>Compares self properties but not the parents.</summary>
    public bool EqualsWithoutParent(Request other) =>
        other.ServiceType == ServiceType
        && other.RequiredServiceType == RequiredServiceType
        && other.IfUnresolved == IfUnresolved

        && other.FactoryType == FactoryType
        && other.ImplementationType == ImplementationType

        && Equals(other.ServiceKey, ServiceKey)
        && Equals(other.MetadataKey, MetadataKey)
        && Equals(other.Metadata, Metadata)

        // todo: Move to Reuse?
        && other.Reuse?.GetType() == Reuse?.GetType()
        && other.Reuse?.Lifespan == Reuse?.Lifespan
        && Equals(other.Reuse?.Name, Reuse?.Name);

    /// <summary>Calculates the combined hash code based on factory IDs.</summary>
    public override int GetHashCode() => _hashCode;

    // Initial request without factory info yet
    private Request(IContainer container, Request parent, int dependencyDepth, int dependencyCount,
        RequestStack stack, RequestFlags flags, IServiceInfo serviceInfo, Expression[] inputArgExprs)
    {
      DirectParent = parent;
      DependencyDepth = dependencyDepth;
      DependencyCount = dependencyCount;
      RequestStack = stack;
      _serviceInfo = serviceInfo;
      _actualServiceType = serviceInfo.GetActualServiceType();
      Flags = flags;

      // runtime state
      InputArgExprs = inputArgExprs;
      Container = container;
    }

    // Request with resolved factory state
    private Request(IContainer container,
        Request parent, int dependencyDepth, int dependencyCount, RequestStack stack,
        RequestFlags flags, IServiceInfo serviceInfo, Expression[] inputArgExprs,
        Type factoryImplType, Factory factory, int factoryID, FactoryType factoryType, IReuse reuse, int decoratedFactoryID)
        : this(container, parent, dependencyDepth, dependencyCount, stack, flags, serviceInfo, inputArgExprs)
    {
      SetResolvedFactory(factoryImplType, factory, factoryID, factoryType, reuse, decoratedFactoryID);
    }

    /// Severe the connection with the request pool up to the parent so that noone can change the Request state
    internal Request IsolateRequestChain()
    {
      Request r = null;
      do
      {
        r = r == null ? this : r.DirectParent;
        if (r.RequestStack != null)
        {
          // severe the requests links with the stack
          r.RequestStack.Items[r.IndexInStack] = null;
          r.RequestStack = null;
        }

      } while ((r.Flags & RequestFlags.IsResolutionCall) == 0 && !r.DirectParent.IsEmpty);

      return this;
    }

    private void SetServiceInfo(IContainer container, Request parent, int dependencyDepth, int dependencyCount,
        RequestStack stack, RequestFlags flags, IServiceInfo serviceInfo, Expression[] inputArgExprs)
    {
      DirectParent = parent;
      DependencyDepth = dependencyDepth;
      DependencyCount = dependencyCount;
      RequestStack = stack;
      _serviceInfo = serviceInfo;
      _actualServiceType = serviceInfo.GetActualServiceType();
      Flags = flags;

      // runtime state
      InputArgExprs = inputArgExprs;
      Container = container;

      // reset factory info
      SetResolvedFactory(null, null, 0, FactoryType.Service, null, 0);
    }

    private void SetResolvedFactory(Type factoryImplType,
        Factory factory, int factoryID, FactoryType factoryType, IReuse reuse, int decoratedFactoryID)
    {
      FactoryID = factoryID;
      FactoryType = factoryType;
      Reuse = reuse;
      DecoratedFactoryID = decoratedFactoryID;
      _hashCode = Hasher.Combine(DirectParent?._hashCode ?? 0, FactoryID);
      _factoryImplType = factoryImplType; // should be set from the runtime known `Factory` object
      Factory = factory; // runtime state
    }
  }

  /// <summary>Type of services supported by Container.</summary>
  public enum FactoryType
  {
    /// <summary>(default) Defines normal service factory</summary>
    Service,
    /// <summary>Defines decorator factory</summary>
    Decorator,
    /// <summary>Defines wrapper factory.</summary>
    Wrapper
  };

  /// <summary>Base class to store optional <see cref="Factory"/> settings.</summary>
  public abstract class Setup
  {
    /// <summary>Factory type is required to be specified by concrete setups as in
    /// <see cref="ServiceSetup"/>, <see cref="DecoratorSetup"/>, <see cref="WrapperSetup"/>.</summary>
    public abstract FactoryType FactoryType { get; }

    /// <summary>Predicate to check if factory could be used for resolved request.</summary>
    public Func<Request, bool> Condition { get; }

    /// <summary>Relative disposal order when defined. Greater number, later dispose.</summary>
    public int DisposalOrder { get; }

    /// <summary>Arbitrary metadata object associated with Factory/Implementation, may be a dictionary of key-values.</summary>
    public virtual object Metadata => null;

    /// <summary>Returns true if passed meta key and value match the setup metadata.</summary>
    public bool MatchesMetadata(string metadataKey, object metadata)
    {
      if (metadataKey == null)
        return Equals(metadata, Metadata);

      object metaValue;
      var metaDict = Metadata as IDictionary<string, object>;
      return metaDict != null
          && metaDict.TryGetValue(metadataKey, out metaValue)
          && Equals(metadata, metaValue);
    }

    /// <summary>Indicates that injected expression should be:
    /// <c><![CDATA[r.Resolver.Resolve<IDependency>(...)]]></c>
    /// instead of: <c><![CDATA[new Dependency(...)]]></c></summary>
    public bool AsResolutionCall => (_settings & Settings.AsResolutionCall) != 0;

    /// Setup with the only setting: `AsResolutionCall` 
    internal static readonly Setup AsResolutionCallSetup =
        new ServiceSetup { _settings = Settings.AsResolutionCall };

    internal Setup WithAsResolutionCall()
    {
      if (AsResolutionCall)
        return this;

      if (this == Default)
        return AsResolutionCallSetup;

      var setupClone = (Setup)MemberwiseClone();
      setupClone._settings |= Settings.AsResolutionCall;
      return setupClone;
    }

    /// <summary>Works as `AsResolutionCall` but only with `Rules.UsedForExpressionGeneration`</summary>
    public bool AsResolutionCallForExpressionGeneration => (_settings & Settings.AsResolutionCallForExpressionGeneration) != 0;

    /// <summary>Specifies to use `asResolutionCall` but only in expression generation context, e.g. for compile-time generation</summary>
    internal static readonly Setup AsResolutionCallForGeneratedExpressionSetup =
        new ServiceSetup { _settings = Settings.AsResolutionCallForExpressionGeneration };

    internal Setup WithAsResolutionCallForGeneratedExpression()
    {
      if (AsResolutionCallForExpressionGeneration)
        return this;

      if (this == Default)
        return AsResolutionCallForGeneratedExpressionSetup;

      var setupClone = (Setup)MemberwiseClone();
      setupClone._settings |= Settings.AsResolutionCallForExpressionGeneration;
      return setupClone;
    }

    /// <summary>Marks service (not a wrapper or decorator) registration that is expected to be resolved via Resolve call.</summary>
    public bool AsResolutionRoot => (_settings & Settings.AsResolutionRoot) != 0;

    /// <summary>Opens scope, also implies <see cref="AsResolutionCall"/>.</summary>
    public bool OpenResolutionScope => (_settings & Settings.OpenResolutionScope) != 0;

    /// <summary>Stores reused instance as WeakReference.</summary>
    public bool WeaklyReferenced => (_settings & Settings.WeaklyReferenced) != 0;

    /// <summary>Allows registering transient disposable.</summary>
    public bool AllowDisposableTransient => (_settings & Settings.AllowDisposableTransient) != 0;

    /// <summary>Turns On tracking of disposable transient dependency in parent scope or in open scope if resolved directly.</summary>
    public bool TrackDisposableTransient => (_settings & Settings.TrackDisposableTransient) != 0;

    /// <summary>Instructs to use parent reuse. Applied only if <see cref="Factory.Reuse"/> is not specified.</summary>
    public bool UseParentReuse => (_settings & Settings.UseParentReuse) != 0;

    /// <summary>Prevents disposal of reused instance if it is disposable.</summary>
    public bool PreventDisposal => (_settings & Settings.PreventDisposal) != 0;

    /// <summary>When single service is resolved, but multiple candidates found, this options will be used to prefer this one.</summary>
    public bool PreferInSingleServiceResolve => (_settings & Settings.PreferInSingleServiceResolve) != 0;

    private Setup() { }

    private Setup(Func<Request, bool> condition,
        bool openResolutionScope, bool asResolutionCall, bool asResolutionRoot, bool preventDisposal, bool weaklyReferenced,
        bool allowDisposableTransient, bool trackDisposableTransient, bool useParentReuse, int disposalOrder,
        bool preferOverMultipleResolved = false, bool asResolutionCallForExpressionGeneration = false)
    {
      Condition = condition;
      DisposalOrder = disposalOrder;

      if (asResolutionCall)
        _settings |= Settings.AsResolutionCall;
      if (openResolutionScope)
      {
        _settings |= Settings.OpenResolutionScope;
        _settings |= Settings.AsResolutionCall;
      }
      if (preventDisposal)
        _settings |= Settings.PreventDisposal;
      if (weaklyReferenced)
        _settings |= Settings.WeaklyReferenced;
      if (allowDisposableTransient)
        _settings |= Settings.AllowDisposableTransient;
      if (trackDisposableTransient)
      {
        _settings |= Settings.TrackDisposableTransient;
        _settings |= Settings.AllowDisposableTransient;
      }
      if (asResolutionRoot)
        _settings |= Settings.AsResolutionRoot;
      if (useParentReuse)
        _settings |= Settings.UseParentReuse;
      if (preferOverMultipleResolved)
        _settings |= Settings.PreferInSingleServiceResolve;
      if (asResolutionCallForExpressionGeneration)
        _settings |= Settings.AsResolutionCallForExpressionGeneration;
    }

    [Flags]
    private enum Settings
    {
      AsResolutionCall = 1 << 1,
      OpenResolutionScope = 1 << 2,
      PreventDisposal = 1 << 3,
      WeaklyReferenced = 1 << 4,
      AllowDisposableTransient = 1 << 5,
      TrackDisposableTransient = 1 << 6,
      AsResolutionRoot = 1 << 7,
      UseParentReuse = 1 << 8,
      PreferInSingleServiceResolve = 1 << 9,
      AsResolutionCallForExpressionGeneration = 1 << 10
    }

    private Settings _settings; // note: mutable because of setting the AsResolutionCall

    /// <summary>Default setup for service factories.</summary>
    public static readonly Setup Default = new ServiceSetup();

    /// <summary>Constructs setup object out of specified settings.
    /// If all settings are default then <see cref="Default"/> setup will be returned.
    /// <paramref name="metadataOrFuncOfMetadata"/> is metadata object or Func returning metadata object.</summary>
    public static Setup With(
        object metadataOrFuncOfMetadata = null, Func<Request, bool> condition = null,
        bool openResolutionScope = false, bool asResolutionCall = false, bool asResolutionRoot = false,
        bool preventDisposal = false, bool weaklyReferenced = false,
        bool allowDisposableTransient = false, bool trackDisposableTransient = false,
        bool useParentReuse = false, int disposalOrder = 0, bool preferInSingleServiceResolve = false)
    {
      if (metadataOrFuncOfMetadata == null && condition == null &&
          !openResolutionScope && !asResolutionRoot &&
          !preventDisposal && !weaklyReferenced && !allowDisposableTransient && !trackDisposableTransient &&
          !useParentReuse && disposalOrder == 0 && !preferInSingleServiceResolve)
        return !asResolutionCall ? Default : AsResolutionCallSetup;

      return new ServiceSetup(condition,
          metadataOrFuncOfMetadata, openResolutionScope, asResolutionCall, asResolutionRoot,
          preventDisposal, weaklyReferenced, allowDisposableTransient, trackDisposableTransient,
          useParentReuse, disposalOrder, preferInSingleServiceResolve);
    }

    /// <summary>Default setup which will look for wrapped service type as single generic parameter.</summary>
    public static readonly Setup Wrapper = new WrapperSetup();

    // todo: rename to WrapperOf
    /// <summary>Returns generic wrapper setup.
    /// Default for <paramref name="wrappedServiceTypeArgIndex" /> is -1 for generic wrapper with single type argument.
    /// Index need to be set for multiple type arguments. <paramref name="alwaysWrapsRequiredServiceType" /> need to be set 
    /// when generic wrapper type arguments should be ignored.</summary>
    public static Setup WrapperWith(int wrappedServiceTypeArgIndex = -1,
        bool alwaysWrapsRequiredServiceType = false, Func<Type, Type> unwrap = null,
        bool openResolutionScope = false, bool asResolutionCall = false,
        bool preventDisposal = false, bool weaklyReferenced = false,
        bool allowDisposableTransient = false, bool trackDisposableTransient = false,
        bool useParentReuse = false, Func<Request, bool> condition = null, int disposalOrder = 0) =>
            wrappedServiceTypeArgIndex == -1 && !alwaysWrapsRequiredServiceType && unwrap == null &&
            !openResolutionScope && !asResolutionCall && !preventDisposal && !weaklyReferenced &&
            !allowDisposableTransient && !trackDisposableTransient && condition == null && disposalOrder == 0
                ? Wrapper
                : new WrapperSetup(wrappedServiceTypeArgIndex, alwaysWrapsRequiredServiceType, unwrap,
                    condition, openResolutionScope, asResolutionCall, preventDisposal, weaklyReferenced,
                    allowDisposableTransient, trackDisposableTransient, useParentReuse, disposalOrder);

    /// <summary>Default decorator setup: decorator is applied to service type it registered with.</summary>
    public static readonly Setup Decorator = new DecoratorSetup();

    // todo: Make decorateeReuse a default?
    /// <summary>Creates setup with optional condition.
    /// The <paramref name="order" /> specifies relative decorator position in decorators chain.
    /// Greater number means further from decoratee - specify negative number to stay closer.
    /// Decorators without order (Order is 0) or with equal order are applied in registration order
    /// - first registered are closer decoratee.</summary>
    public static Setup DecoratorWith(
        Func<Request, bool> condition = null, int order = 0, bool useDecorateeReuse = false,
        bool openResolutionScope = false, bool asResolutionCall = false,
        bool preventDisposal = false, bool weaklyReferenced = false,
        bool allowDisposableTransient = false, bool trackDisposableTransient = false,
        int disposalOrder = 0) =>
        condition == null && order == 0 && !useDecorateeReuse &&
        !openResolutionScope && !asResolutionCall &&
        !preventDisposal && !weaklyReferenced && !allowDisposableTransient && !trackDisposableTransient &&
        disposalOrder == 0
            ? Decorator
            : new DecoratorSetup(condition, order, useDecorateeReuse, openResolutionScope, asResolutionCall,
                preventDisposal, weaklyReferenced, allowDisposableTransient, trackDisposableTransient, disposalOrder);

    /// Creates a condition for both <paramref name="decorateeType"/>, <paramref name="decorateeServiceKey"/> and additional condition
    public static Func<Request, bool> GetDecorateeCondition(Type decorateeType,
        object decorateeServiceKey = null, Func<Request, bool> condition = null)
    {
      if (decorateeType == null && decorateeServiceKey == null)
        return condition;

      Func<Request, bool> decorateeCondition;
      if (decorateeServiceKey == null)
        decorateeCondition = r => r.GetKnownImplementationOrServiceType().IsAssignableTo(decorateeType);
      else if (decorateeType == null)
        decorateeCondition = r => decorateeServiceKey.Equals(r.ServiceKey);
      else
        decorateeCondition = r => decorateeServiceKey.Equals(r.ServiceKey) &&
                             r.GetKnownImplementationOrServiceType().IsAssignableTo(decorateeType);
      return condition == null ? decorateeCondition : r => decorateeCondition(r) && condition(r);
    }

    /// <summary>Setup for decorator of type <paramref name="decorateeType"/>.</summary>
    public static Setup DecoratorOf(Type decorateeType = null,
        int order = 0, bool useDecorateeReuse = false, bool openResolutionScope = false, bool asResolutionCall = false,
        bool preventDisposal = false, bool weaklyReferenced = false, bool allowDisposableTransient = false,
        bool trackDisposableTransient = false, int disposalOrder = 0, object decorateeServiceKey = null) =>
        DecoratorWith(GetDecorateeCondition(decorateeType, decorateeServiceKey), order, useDecorateeReuse, openResolutionScope, asResolutionCall,
            preventDisposal, weaklyReferenced, allowDisposableTransient, trackDisposableTransient, disposalOrder);

    /// <summary>Setup for decorator of type <typeparamref name="TDecoratee"/>.</summary>
    public static Setup DecoratorOf<TDecoratee>(
        int order = 0, bool useDecorateeReuse = false, bool openResolutionScope = false, bool asResolutionCall = false,
        bool preventDisposal = false, bool weaklyReferenced = false, bool allowDisposableTransient = false,
        bool trackDisposableTransient = false, int disposalOrder = 0, object decorateeServiceKey = null) =>
        DecoratorOf(typeof(TDecoratee), order, useDecorateeReuse, openResolutionScope, asResolutionCall,
            preventDisposal, weaklyReferenced, allowDisposableTransient, trackDisposableTransient,
            disposalOrder, decorateeServiceKey);

    /// <summary>Service setup.</summary>
    internal sealed class ServiceSetup : Setup
    {
      /// <inheritdoc />
      public override FactoryType FactoryType => FactoryType.Service;

      /// <summary>Evaluates metadata if it specified as Func of object, and replaces Func with its result!.
      /// Otherwise just returns metadata object.</summary>
      /// <remarks>Invocation of Func metadata is Not thread-safe. Please take care of that inside the Func.</remarks>
      public override object Metadata =>
          _metadataOrFuncOfMetadata is Func<object> metaFactory
              ? (_metadataOrFuncOfMetadata = metaFactory())
              : _metadataOrFuncOfMetadata;

      /// All settings are set to defaults.
      public ServiceSetup() { }

      /// Specify all the individual settings.
      public ServiceSetup(Func<Request, bool> condition = null, object metadataOrFuncOfMetadata = null,
          bool openResolutionScope = false, bool asResolutionCall = false, bool asResolutionRoot = false,
          bool preventDisposal = false, bool weaklyReferenced = false, bool allowDisposableTransient = false,
          bool trackDisposableTransient = false, bool useParentReuse = false, int disposalOrder = 0,
          bool preferOverMultipleResolved = false, bool asResolutionCallForExpressionGeneration = false)
          : base(condition, openResolutionScope, asResolutionCall, asResolutionRoot,
              preventDisposal, weaklyReferenced, allowDisposableTransient, trackDisposableTransient,
              useParentReuse, disposalOrder, preferOverMultipleResolved, asResolutionCallForExpressionGeneration)
      {
        _metadataOrFuncOfMetadata = metadataOrFuncOfMetadata;
      }

      private object _metadataOrFuncOfMetadata;
    }

    /// <summary>Setup applied for wrappers.</summary>
    internal sealed class WrapperSetup : Setup
    {
      /// <summary>Returns <see cref="Injector.FactoryType.Wrapper"/> type.</summary>
      public override FactoryType FactoryType => FactoryType.Wrapper;

      /// <summary>Delegate to get wrapped type from provided wrapper type.
      /// If wrapper is generic, then wrapped type is usually a generic parameter.</summary>
      public readonly int WrappedServiceTypeArgIndex;

      /// <summary>Per name.</summary>
      public readonly bool AlwaysWrapsRequiredServiceType;

      /// <summary>Delegate returning wrapped type from wrapper type. Overwrites other options.</summary>
      public readonly Func<Type, Type> Unwrap;

      /// <summary>Default setup</summary>
      /// <param name="wrappedServiceTypeArgIndex">Default is -1 for generic wrapper with single type argument.
      /// Need to be set for multiple type arguments.</param>
      public WrapperSetup(int wrappedServiceTypeArgIndex = -1)
      {
        WrappedServiceTypeArgIndex = wrappedServiceTypeArgIndex;
      }

      /// <summary>Returns generic wrapper setup.
      /// Default for <paramref name="wrappedServiceTypeArgIndex" /> is -1 for generic wrapper with single type argument.
      /// Index need to be set for multiple type arguments. <paramref name="alwaysWrapsRequiredServiceType" /> need to be set 
      /// when generic wrapper type arguments should be ignored.</summary>
      public WrapperSetup(int wrappedServiceTypeArgIndex, bool alwaysWrapsRequiredServiceType, Func<Type, Type> unwrap,
          Func<Request, bool> condition,
          bool openResolutionScope, bool asResolutionCall,
          bool preventDisposal, bool weaklyReferenced, bool allowDisposableTransient, bool trackDisposableTransient,
          bool useParentReuse, int disposalOrder)
          : base(condition, openResolutionScope, asResolutionCall, false, preventDisposal, weaklyReferenced,
              allowDisposableTransient, trackDisposableTransient, useParentReuse, disposalOrder)
      {
        WrappedServiceTypeArgIndex = wrappedServiceTypeArgIndex;
        AlwaysWrapsRequiredServiceType = alwaysWrapsRequiredServiceType;
        Unwrap = unwrap;
      }

      internal void ThrowIfInvalidRegistration(Type serviceType)
      {
        if (AlwaysWrapsRequiredServiceType || Unwrap != null || !serviceType.IsGeneric())
          return;

        var typeArgCount = serviceType.GetGenericParamsAndArgs().Length;
        var typeArgIndex = WrappedServiceTypeArgIndex;
        Throw.If(typeArgCount > 1 && typeArgIndex == -1,
            Error.GenericWrapperWithMultipleTypeArgsShouldSpecifyArgIndex, serviceType);

        var index = typeArgIndex != -1 ? typeArgIndex : 0;
        Throw.If(index > typeArgCount - 1,
            Error.GenericWrapperTypeArgIndexOutOfBounds, serviceType, index);
      }

      /// <summary>Unwraps service type or returns the <paramref name="serviceType"/> as-is.</summary>
      public Type GetWrappedTypeOrNullIfWrapsRequired(Type serviceType)
      {
        if (Unwrap != null)
          return Unwrap(serviceType);

        if (AlwaysWrapsRequiredServiceType || !serviceType.IsGeneric())
          return null;

        var typeArgs = serviceType.GetGenericParamsAndArgs();
        var typeArgIndex = WrappedServiceTypeArgIndex;
        serviceType.ThrowIf(typeArgs.Length > 1 && typeArgIndex == -1,
            Error.GenericWrapperWithMultipleTypeArgsShouldSpecifyArgIndex);

        typeArgIndex = typeArgIndex != -1 ? typeArgIndex : 0;
        serviceType.ThrowIf(typeArgIndex > typeArgs.Length - 1,
            Error.GenericWrapperTypeArgIndexOutOfBounds, typeArgIndex);

        return typeArgs[typeArgIndex];
      }
    }

    /// <summary>Setup applied to decorators.</summary>
    internal sealed class DecoratorSetup : Setup
    {
      /// <summary>Returns Decorator factory type.</summary>
      public override FactoryType FactoryType => FactoryType.Decorator;

      /// <summary>If provided specifies relative decorator position in decorators chain.
      /// Greater number means further from decoratee - specify negative number to stay closer.
      /// Decorators without order (Order is 0) or with equal order are applied in registration order
      /// - first registered are closer decoratee.</summary>
      public readonly int Order;

      // todo: It does not consider the keys of decorated services,
      // therefore it will be shared between all services in collection
      /// <summary>Instructs to use decorated service reuse. Decorated service may be decorator itself.</summary>
      public readonly bool UseDecorateeReuse;

      /// <summary>Default setup.</summary>
      public DecoratorSetup() { }

      /// <summary>Creates decorator setup with optional condition. <paramref name="condition" /> applied to 
      /// decorated service to find that service is the decorator target. <paramref name="order" /> specifies 
      /// relative decorator position in decorators chain. Greater number means further from decoratee -
      /// specify negative number to stay closer. Decorators without order (Order is 0) or with equal order
      /// are applied in registration order - first registered are closer decoratee.</summary>
      public DecoratorSetup(Func<Request, bool> condition, int order, bool useDecorateeReuse,
          bool openResolutionScope = false, bool asResolutionCall = false,
          bool preventDisposal = false, bool weaklyReferenced = false,
          bool allowDisposableTransient = false, bool trackDisposableTransient = false,
          int disposalOrder = 0)
          : base(condition, openResolutionScope, asResolutionCall, false, preventDisposal, weaklyReferenced,
              allowDisposableTransient, trackDisposableTransient, false, disposalOrder)
      {
        Order = order;
        UseDecorateeReuse = useDecorateeReuse;
      }
    }
  }

  /// <summary>Facility for creating concrete factories from some template/prototype. Example:
  /// creating closed-generic type reflection factory from registered open-generic prototype factory.</summary>
  public interface IConcreteFactoryGenerator
  {
    // todo: @perf @v5 make it a ImHashMap<object, object> to use the implementationType as a key for no or default service key
    /// <summary>Generated factories so far, identified by the service type and key pair.</summary>
    ImHashMap<KV<Type, object>, ReflectionFactory> GeneratedFactories { get; }

    /// <summary>Returns factory per request. May track already generated factories and return one without regenerating.</summary>
    Factory GetGeneratedFactory(Request request, bool ifErrorReturnDefault = false);
  }

  /// Instructs how to deal with factory result expression: 
  public enum FactoryCaching
  {   /// Is up to DryIoc to decide,
    Default = 0,
    /// Prevents DryIoc to set `DoNotCache`.
    PleaseDontSetDoNotCache,
    /// If set, the expression won't be cached 
    DoNotCache
  }

  /// <summary>Base class for different ways to instantiate service:
  /// <list type="bullet">
  /// <item>Through reflection - <see cref="ReflectionFactory"/></item>
  /// <item>Using custom delegate - <see cref="DelegateFactory"/></item>
  /// <item>Using custom expression - <see cref="ExpressionFactory"/></item>
  /// <item>A placeholder for future actual implementation - <see cref="FactoryPlaceholder"/></item>
  /// </list>
  /// For all of the types Factory should provide result as <see cref="Expression"/> and <see cref="FactoryDelegate"/>.
  /// Factories are supposed to be immutable and stateless.
  /// Each created factory has an unique ID set in <see cref="FactoryID"/>.</summary>
  public abstract class Factory
  {
    /// <summary>Get next factory ID in a atomic way.</summary><returns>The ID.</returns>
    public static int GetNextID() => Interlocked.Increment(ref _lastFactoryID);

    /// <summary>Unique factory id generated from static seed.</summary>
    public int FactoryID { get; internal set; }

    /// <summary>Reuse policy for created services.</summary>
    public virtual IReuse Reuse => _reuse;

    /// <summary>Setup may contain different/non-default factory settings.</summary>
    public virtual Setup Setup
    {
      get => _setup;
      internal set { _setup = value ?? Setup.Default; }
    }

    /// <summary>Checks that condition is met for request or there is no condition setup.</summary>
    public bool CheckCondition(Request request)
    {
      var condition = Setup.Condition;
      return condition == null || condition(request);
    }

    /// <summary>Shortcut for <see cref="Injector.Setup.FactoryType"/>.</summary>
    public FactoryType FactoryType => Setup.FactoryType;

    /// <summary>Non-abstract closed implementation type. May be null if not known beforehand, e.g. in <see cref="DelegateFactory"/>.</summary>
    public virtual Type ImplementationType => null;

    /// <summary>Allow inheritors to define lazy implementation type</summary>
    public virtual bool CanAccessImplementationType => true;

    /// <summary>Indicates that Factory is factory provider and
    /// consumer should call <see cref="IConcreteFactoryGenerator.GetGeneratedFactory"/> to get concrete factory.</summary>
    public virtual IConcreteFactoryGenerator FactoryGenerator => null;

    /// <summary>Registration order.</summary>
    public virtual int RegistrationOrder => FactoryID;

    /// <summary>Settings <b>(if any)</b> to select Constructor/FactoryMethod, Parameters, Properties and Fields.</summary>
    public virtual Made Made => Made.Default;

    /// <summary>The factory inserts the runtime-state into result expression, e.g. delegate or pre-created instance.</summary>
    public virtual bool HasRuntimeState => false;

    /// Indicates how to deal with the result expression
    public FactoryCaching Caching { get; set; }

    /// Instructs to skip caching the factory unless it really wants to do so via `PleaseDontSetDoNotCache`
    public Factory DoNotCache()
    {
      if (Caching != FactoryCaching.PleaseDontSetDoNotCache)
        Caching = FactoryCaching.DoNotCache;
      return this;
    }

    /// <summary>Initializes reuse and setup. Sets the <see cref="FactoryID"/></summary>
    /// <param name="reuse">(optional)</param> <param name="setup">(optional)</param>
    protected Factory(IReuse reuse = null, Setup setup = null)
    {
      FactoryID = GetNextID();
      _reuse = reuse;
      _setup = setup ?? Setup.Default;
    }

    /// <summary>The main factory method to create service expression, e.g. "new Client(new Service())".
    /// If <paramref name="request"/> has <see cref="Request.InputArgExprs"/> specified, they could be used in expression.</summary>
    /// <param name="request">Service request.</param>
    /// <returns>Created expression.</returns>
    public abstract Expression CreateExpressionOrDefault(Request request);

    /// <summary>Returns service expression: either by creating it with <see cref="CreateExpressionOrDefault"/> or taking expression from cache.
    /// Before returning method may transform the expression  by applying <see cref="Reuse"/>, or/and decorators if found any.</summary>
    public virtual Expression GetExpressionOrDefault(Request request)
    {
      request = request.WithResolvedFactory(this);

      // First look for decorators if it is not already a decorator
      var container = request.Container;
      if (FactoryType != FactoryType.Decorator)
      {
        var decoratorExpr = container.GetDecoratorExpressionOrDefault(request);
        if (decoratorExpr != null)
          return decoratorExpr;
      }

      var setup = Setup;
      var rules = container.Rules;

      var getAsRsolutionCall =
          (request.Flags & RequestFlags.IsGeneratedResolutionDependencyExpression) == 0
          && !request.OpensResolutionScope
          && (setup.OpenResolutionScope ||
              !request.IsResolutionCall
                  && (setup.AsResolutionCall || (setup.AsResolutionCallForExpressionGeneration && rules.UsedForExpressionGeneration))
                  && request.GetActualServiceType() != typeof(void));

      if (getAsRsolutionCall)
        return Resolver.CreateResolutionExpression(request, setup.OpenResolutionScope, setup.AsResolutionCall);

      // todo: @perf check for the false non-caching
      var cacheExpression =
          Caching != FactoryCaching.DoNotCache &&
          FactoryType == FactoryType.Service &&
          !request.IsResolutionRoot &&
          !request.IsDirectlyWrappedInFunc() &&
          !request.IsWrappedInFuncWithArgs() &&
          !(request.Reuse.Name is IScopeName) &&
          !setup.AsResolutionCall && // see #295
          !setup.UseParentReuse &&
          setup.Condition == null &&
          !Made.IsConditional;

      // First, lookup in the expression cache
      var reuse = request.Reuse;
      ImMapEntry<Container.Registry.ExpressionCacheSlot> cacheEntry = null;
      if (cacheExpression)
      {
        var cachedExpr = ((Container)container).GetCachedFactoryExpression(request.FactoryID, reuse, out cacheEntry);
        if (cachedExpr != null)
        {
          if (reuse == Crystal.Reuse.Transient &&
              cacheEntry.Value.ScopeNameOrDependencyCount is Container.Registry.TransientDependencyCount depCount &&
              depCount.Value > 0 &&
              !rules.UsedForValidation && !rules.UsedForExpressionGeneration)
          {
            for (var p = request.DirectParent; !p.IsEmpty; p = p.DirectParent)
              p.DependencyCount += depCount.Value;
          }
          return cachedExpr;
        }
      }

      // Next, lookup for the already created service in the singleton scope
      Expression serviceExpr;
      if (request.Reuse is SingletonReuse && request.Rules.EagerCachingSingletonForFasterAccess)
      {
        // Then optimize for already resolved singleton object, otherwise goes normal ApplyReuse route
        var id = request.FactoryType == FactoryType.Decorator
            ? request.CombineDecoratorWithDecoratedFactoryID()
            : request.FactoryID;

        var itemRef = ((Scope)container.SingletonScope)._maps[id & Scope.MAP_COUNT_SUFFIX_MASK]
            .GetEntryOrDefault(id);
        if (itemRef != null)
        {
          // Note: (for details check the test for #340 and the `For_singleton_can_use_func_without_args_or_just_resolve_after_func_with_args`)
          //
          // Now we are in the danger zone if try to get the singleton wrapped in Func as a dependency of the same singleton, e.g.
          //
          // `class Singleton { public Singleton(Func<Singleton> fs) {} }`
          //
          // In this situation the `itemRef` will stuck forever on the `WaitForItemIsSet` below.
          // So the way-out here is to abondon the attempt to wait for the item if we are not in the Func
          // and proceed to the normal Expression creation.
          //
          if (itemRef.Value == Scope.NoItem)
          {
            if (!request.TracksTransientDisposable && !request.IsWrappedInFunc())
              Scope.WaitForItemIsSet(itemRef);
          }

          if (itemRef.Value != Scope.NoItem) // get the item if and only if it is already created
          {
            var singleton = itemRef.Value;
            serviceExpr = singleton != null ? Constant(singleton)
                : Constant(null, request.GetActualServiceType()); // fixes #258

            if (Setup.WeaklyReferenced) // Unwrap WeakReference or HiddenDisposable in that order!
              serviceExpr = Call(ThrowInGeneratedCode.WeakRefReuseWrapperGCedMethod,
                  Property(Convert(serviceExpr, typeof(WeakReference)), ThrowInGeneratedCode.WeakReferenceValueProperty));
            else if (Setup.PreventDisposal)
              serviceExpr = Field(Convert(serviceExpr, typeof(HiddenDisposable)), HiddenDisposable.ValueField);

            return serviceExpr;
          }
        }
      }

      // At last, create the object graph with all of the dependencies created and injected
      serviceExpr = CreateExpressionOrDefault(request);
      if (serviceExpr == null)
      {
        Container.TryThrowUnableToResolve(request);
        return null;
      }

      if (reuse != Crystal.Reuse.Transient)
      {
        if (!rules.UsedForValidation &&
            request.GetActualServiceType() != typeof(void))
        {
          var originalServiceExprType = serviceExpr.Type;

          serviceExpr = ApplyReuse(serviceExpr, request); // todo: @perf pass the possibly claculated id to here

          if (serviceExpr.NodeType != ExprType.Constant &&
              serviceExpr.Type != originalServiceExprType &&
              !originalServiceExprType.GetTypeInfo().IsAssignableFrom(serviceExpr.Type.GetTypeInfo()))
            serviceExpr = Convert(serviceExpr, originalServiceExprType);
        }
      }
      else if (!rules.UsedForValidation &&
               !rules.UsedForExpressionGeneration)
      {
        // Split the expression with dependencies bigger than certain threshold by wrapping it in Func which is a
        // separate compilation unit and invoking it emmediately
        var depCount = request.DependencyCount;
        if (depCount >= rules.DependencyCountInLambdaToSplitBigObjectGraph)
        {
          request.DecreaseTrackedDependencyCountForParents(depCount);

          if (rules.UseFastExpressionCompiler)
          {
            serviceExpr = Convert(Invoke(
                Lambda(typeof(Func<object>), serviceExpr, Empty<ParameterExpression>()
#if SUPPORTS_FAST_EXPRESSION_COMPILER
                                , typeof(object)
#endif
                            ), Empty<Expression>()), serviceExpr.Type);
          }
          else
          {
            // cache expression if possible to minimize the double work for the generated Resolve call
            if (cacheExpression)
              ((Container)container).CacheFactoryExpression(request.FactoryID, serviceExpr, reuse,
                  depCount, cacheEntry);

            return Resolver.CreateResolutionExpression(request);
          }
        }
      }

      if (cacheExpression)
        ((Container)container).CacheFactoryExpression(request.FactoryID, serviceExpr, reuse,
            reuse == Crystal.Reuse.Transient ? request.DependencyCount : 0,
            cacheEntry);

      return serviceExpr;
    }

    /// <summary>Applies reuse to created expression, by wrapping passed expression into scoped access
    /// and producing the result expression.</summary>
    protected virtual Expression ApplyReuse(Expression serviceExpr, Request request)
    {
      // This optimization eagerly creates singleton during the construction of object graph
      // Singleton is created once and then is stred for the container lifetime (until Сontainer.SingletonScope is disposed).
      // That's why we are always intepreting them even if `Rules.WithoutInterpretationForTheFirstResolution()` is set.
      if (request.Reuse is SingletonReuse && request.Rules.EagerCachingSingletonForFasterAccess &&
          !request.TracksTransientDisposable && !request.IsWrappedInFunc())
      {
        var container = request.Container;
        var scope = (Scope)container.SingletonScope;
        if (scope.IsDisposed)
          Throw.It(Error.ScopeIsDisposed, scope.ToString());

        var id = request.FactoryType == FactoryType.Decorator
            ? request.CombineDecoratorWithDecoratedFactoryID()
            : request.FactoryID;

        var singleton = Scope.NoItem; // NoItem is a marker for the value not created yet

        // Creating a new local item with the id and the marker for not yet created item
        var itemRef = new ImMapEntry<object>(id, Scope.NoItem);
        ref var map = ref scope._maps[id & Scope.MAP_COUNT_SUFFIX_MASK]; // got a live reference to the map where value can be queried and stored

        var oldMap = map; // get a reference to the map available now as an oldMap
        var newMap = oldMap.AddOrKeepEntry(itemRef);

        // If the `newMap` is the same as an `oldMap` it means there is item already in the map.
        // The check before the CAS operation is only here for Singleton and not for the scope 
        // because the race for the Singletons and the situation where singleton is already create in parallel 
        // is far more likely and the race for the Scoped services is almost non-existent 
        // (because Scoped is almost equal to the single thread or the single invocation flow)
        if (newMap == oldMap)
        {
          // It does not matter if the live `map` changed because the item can only be added and not removed ever
          var otherItemRef = newMap.GetSurePresentEntry(id);
          singleton = otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
        }
        else if (Interlocked.CompareExchange(ref map, newMap, oldMap) != oldMap) // set map to newMap if the map did no change since getting oldMap
        {
          // if the map was changed in parallel, let's retry using the Swap
          newMap = Ref.SwapAndGetNewValue(ref map, itemRef, (x, i) => x.AddOrKeepEntry(i));
          var otherItemRef = newMap.GetSurePresentEntry(id);
          if (otherItemRef != itemRef)
            singleton = otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
        }

        if (singleton == Scope.NoItem)
        {
#if SUPPORTS_SPIN_WAIT
          if (!Interpreter.TryInterpretAndUnwrapContainerException(container, serviceExpr, container.Rules.UseFastExpressionCompiler, out singleton))
            singleton = serviceExpr.CompileToFactoryDelegate(container.Rules.UseFastExpressionCompiler, container.Rules.UseInterpretation)(container);

          if (Setup.WeaklyReferenced)
            singleton = new WeakReference(singleton);
          else if (Setup.PreventDisposal)
            singleton = new HiddenDisposable(singleton); // todo: @perf we don't need it here because because instead of wrapping the item into the non-disposable object we may skip adding it to Disposable items collection - just skipping the AddUnorderedDisposable or AddDisposable calls below
          itemRef.Value = singleton;
#else
                    lock (itemRef)
                    {
                        if (!Interpreter.TryInterpretAndUnwrapContainerException(container, serviceExpr, container.Rules.UseFastExpressionCompiler, out singleton))
                            singleton = serviceExpr.CompileToFactoryDelegate(container.Rules.UseFastExpressionCompiler, container.Rules.UseInterpretation)(container);

                        if (Setup.WeaklyReferenced)
                            singleton = new WeakReference(singleton);
                        // todo: @perf Do we need HiddenDisposable here or instead we may skip adding the object to Disposable items collection 
                        // - just don't call the AddUnorderedDisposable or AddDisposable below.
                        // Huh, but we need to handle the case when we have the expression createdfor not eager singleton.
                        // So maybe we can optimize and simplify for the eager singleton only?
                        else if (Setup.PreventDisposal) 
                            singleton = new HiddenDisposable(singleton); 
                        itemRef.Value = singleton;
                        Monitor.PulseAll(itemRef);
                    }
#endif
          if (singleton is IDisposable disp && !ReferenceEquals(disp, scope))
          {
            if (Setup.DisposalOrder == 0)
              scope.AddUnorderedDisposable(disp);
            else
              scope.AddDisposable(disp, Setup.DisposalOrder);
          }
        }

        Debug.Assert(singleton != Scope.NoItem, "Should not be the case otherwise I am effing failed");
        serviceExpr = singleton == null ? Constant(null, serviceExpr.Type) /* fixes #258 */ : Constant(singleton);
        if (request.DependencyCount > 0)
          request.DecreaseTrackedDependencyCountForParents(request.DependencyCount);
      }
      else
      {
        // Wrap service expression in WeakReference or HiddenDisposable
        if (Setup.WeaklyReferenced)
          serviceExpr = New(ThrowInGeneratedCode.WeakReferenceCtor, serviceExpr);
        else if (Setup.PreventDisposal)
          serviceExpr = New(HiddenDisposable.Ctor, serviceExpr);

        serviceExpr = request.Reuse.Apply(request, serviceExpr);
      }

      if (Setup.WeaklyReferenced) // Unwrap WeakReference or HiddenDisposable in that order!
        serviceExpr = Call(ThrowInGeneratedCode.WeakRefReuseWrapperGCedMethod,
            Property(Convert(serviceExpr, typeof(WeakReference)), ThrowInGeneratedCode.WeakReferenceValueProperty));
      else if (Setup.PreventDisposal)
        serviceExpr = Field(Convert(serviceExpr, typeof(HiddenDisposable)), HiddenDisposable.ValueField);

      return serviceExpr;
    }

    // todo: remove this
    /// [Obsolete("Not need to control on the factory level, the remaining UseInstanceFactory will be removed")] 
    public virtual bool UseInterpretation(Request request) => request.Rules.UseInterpretationForTheFirstResolution;

    /// Creates factory delegate from service expression and returns it.
    public virtual FactoryDelegate GetDelegateOrDefault(Request request) =>
        GetExpressionOrDefault(request)
            ?.CompileToFactoryDelegate(request.Rules.UseFastExpressionCompiler, request.Rules.UseInterpretation);

    internal virtual bool ValidateAndNormalizeRegistration(Type serviceType, object serviceKey, bool isStaticallyChecked, Rules rules)
    {
      if (!isStaticallyChecked)
        serviceType.ThrowIfNull();

      var setup = Setup;
      if (setup.FactoryType == FactoryType.Service)
      {
        // Warn about registering disposable transient
        var reuse = Reuse ?? rules.DefaultReuse;
        if (reuse != Crystal.Reuse.Transient)
          return true;

        if (setup.AllowDisposableTransient ||
            !rules.ThrowOnRegisteringDisposableTransient)
          return true;

        if (setup.UseParentReuse ||
            setup.FactoryType == FactoryType.Decorator && ((Setup.DecoratorSetup)setup).UseDecorateeReuse)
          return true;

        var knownImplOrServiceType = CanAccessImplementationType ? ImplementationType : serviceType;
        if (knownImplOrServiceType.IsAssignableTo<IDisposable>())
          Throw.It(Error.RegisteredDisposableTransientWontBeDisposedByContainer,
              serviceType, serviceKey ?? "{no key}", this);
      }
      else if (setup.FactoryType == FactoryType.Wrapper)
      {
        ((Setup.WrapperSetup)setup).ThrowIfInvalidRegistration(serviceType);
      }
      else if (setup.FactoryType == FactoryType.Decorator)
      {
        if (serviceKey != null)
          Throw.It(Error.DecoratorShouldNotBeRegisteredWithServiceKey, serviceKey);
      }

      return true;
    }

    /// <summary>Returns nice string representation of factory.</summary>
    public override string ToString()
    {
      var s = new StringBuilder().Append("{FactoryID=").Append(FactoryID);
      if (ImplementationType != null)
        s.Append(", ImplType=").Print(ImplementationType);
      if (Reuse != null)
        s.Append(", Reuse=").Print(Reuse);
      if (Setup.FactoryType != Setup.Default.FactoryType)
        s.Append(", FactoryType=").Append(Setup.FactoryType);
      if (Setup.Metadata != null)
        s.Append(", Metadata=").Print(Setup.Metadata);
      if (Setup.Condition != null)
        s.Append(", HasCondition");

      if (Setup.OpenResolutionScope)
        s.Append(", OpensResolutionScope");
      else if (Setup.AsResolutionCall)
        s.Append(", AsResolutionCall");

      return s.Append("}").ToString();
    }

    #region Implementation

    internal static int _lastFactoryID;
    private IReuse _reuse;
    private Setup _setup;

    #endregion
  }

  /// <summary>Declares delegate to get single factory method or constructor for resolved request.</summary>
  public delegate FactoryMethod FactoryMethodSelector(Request request);

  /// <summary>Specifies how to get parameter info for injected parameter and resolved request</summary>
  public delegate Func<ParameterInfo, ParameterServiceInfo> ParameterSelector(Request request);

  /// <summary>Specifies what properties or fields to inject and how.</summary>
  public delegate IEnumerable<PropertyOrFieldServiceInfo> PropertiesAndFieldsSelector(Request request);

  /// <summary>DSL for specifying <see cref="ParameterSelector"/> injection rules.</summary>
  public static class Parameters
  {
    /// <summary>Returns default service info wrapper for each parameter info.</summary>
    public static ParameterSelector Of = request => ParameterServiceInfo.Of;

    /// <summary>Returns service info which considers each parameter as optional.</summary>
    public static ParameterSelector IfUnresolvedReturnDefault =
        request => pi => ParameterServiceInfo.Of(pi).WithDetails(ServiceDetails.IfUnresolvedReturnDefault);

    /// <summary>Combines source selector with other. Other is used as fallback when source returns null.</summary>
    public static ParameterSelector OverrideWith(this ParameterSelector source, ParameterSelector other) =>
        source == null || source == Of ? other ?? Of
        : other == null || other == Of ? source
        : req => paramInfo => other(req)?.Invoke(paramInfo) ?? source(req)?.Invoke(paramInfo);

    /// <summary>Obsolete: please use <see cref="OverrideWith"/></summary>
    [Obsolete("Replaced with OverrideWith", false)]
    public static ParameterSelector And(this ParameterSelector source, ParameterSelector other) =>
        source.OverrideWith(other);

    /// <summary>Overrides source parameter rules with specific parameter details. 
    /// If it is not your parameter just return null.</summary>
    /// <param name="source">Original parameters rules</param>
    /// <param name="getDetailsOrNull">Should return specific details or null.</param>
    /// <returns>New parameters rules.</returns>
    public static ParameterSelector Details(this ParameterSelector source, Func<Request, ParameterInfo, ServiceDetails> getDetailsOrNull)
    {
      getDetailsOrNull.ThrowIfNull();
      return source.OverrideWith(request => p => getDetailsOrNull(request, p)?.To(ParameterServiceInfo.Of(p).WithDetails));
    }

    /// <summary>Adds to <paramref name="source"/> selector service info for parameter identified by <paramref name="name"/>.</summary>
    /// <param name="source">Original parameters rules.</param> <param name="name">Name to identify parameter.</param>
    /// <param name="requiredServiceType">(optional)</param> <param name="serviceKey">(optional)</param>
    /// <param name="ifUnresolved">(optional) By default throws exception if unresolved.</param>
    /// <param name="defaultValue">(optional) Specifies default value to use when unresolved.</param>
    /// <param name="metadataKey">(optional) Required metadata key</param> <param name="metadata">Required metadata or value.</param>
    /// <returns>New parameters rules.</returns>
    public static ParameterSelector Name(this ParameterSelector source, string name,
        Type requiredServiceType = null, object serviceKey = null,
        IfUnresolved ifUnresolved = IfUnresolved.Throw, object defaultValue = null,
        string metadataKey = null, object metadata = null) =>
        source.Details((r, p) => !p.Name.Equals(name) ? null
            : ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, defaultValue, metadataKey, metadata));

    /// <summary>Specify parameter by name and set custom value to it.</summary>
    public static ParameterSelector Name(this ParameterSelector source,
        string name, Func<Request, ParameterInfo, ServiceDetails> getServiceDetails) =>
        source.Details((r, p) => p.Name.Equals(name) ? getServiceDetails(r, p) : null);

    /// <summary>Specify parameter by name and set custom value to it.</summary>
    public static ParameterSelector Name(this ParameterSelector source,
        string name, Func<Request, object> getCustomValue) =>
         source.Name(name, (r, p) => ServiceDetails.Of(getCustomValue(r)));

    /// <summary>Adds to <paramref name="source"/> selector service info for parameter identified by type <paramref name="parameterType"/>.</summary>
    /// <param name="source">Source selector.</param> <param name="parameterType">The type of the parameter.</param>
    /// <param name="requiredServiceType">(optional)</param> <param name="serviceKey">(optional)</param>
    /// <param name="ifUnresolved">(optional) By default throws exception if unresolved.</param>
    /// <param name="defaultValue">(optional) Specifies default value to use when unresolved.</param>
    /// <param name="metadataKey">(optional) Required metadata key</param> <param name="metadata">Required metadata or value.</param>
    /// <returns>Combined selector.</returns>
    public static ParameterSelector Type(this ParameterSelector source, Type parameterType,
        Type requiredServiceType = null, object serviceKey = null,
        IfUnresolved ifUnresolved = IfUnresolved.Throw, object defaultValue = null,
        string metadataKey = null, object metadata = null) =>
        source.Details((r, p) => !parameterType.IsAssignableTo(p.ParameterType) ? null
            : ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, defaultValue, metadataKey, metadata));

    /// <summary>Adds to <paramref name="source"/> selector service info for parameter identified by type <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">Type of parameter.</typeparam> <param name="source">Source selector.</param>
    /// <param name="requiredServiceType">(optional)</param> <param name="serviceKey">(optional)</param>
    /// <param name="ifUnresolved">(optional) By default throws exception if unresolved.</param>
    /// <param name="defaultValue">(optional) Specifies default value to use when unresolved.</param>
    /// <param name="metadataKey">(optional) Required metadata key</param> <param name="metadata">Required metadata or value.</param>
    /// <returns>Combined selector.</returns>
    public static ParameterSelector Type<T>(this ParameterSelector source,
        Type requiredServiceType = null, object serviceKey = null,
        IfUnresolved ifUnresolved = IfUnresolved.Throw, object defaultValue = null,
        string metadataKey = null, object metadata = null) =>
        source.Type(typeof(T), requiredServiceType, serviceKey, ifUnresolved, defaultValue, metadataKey, metadata);

    /// <summary>Specify parameter by type and set its details.</summary>
    public static ParameterSelector Type<T>(this ParameterSelector source,
        Func<Request, ParameterInfo, ServiceDetails> getServiceDetails) =>
        source.Details((r, p) => p.ParameterType == typeof(T) ? getServiceDetails(r, p) : null);

    /// <summary>Specify parameter by type and set custom value to it.</summary>
    public static ParameterSelector Type<T>(this ParameterSelector source, Func<Request, T> getCustomValue) =>
        source.Type<T>((r, p) => ServiceDetails.Of(getCustomValue(r)));

    /// <summary>Specify parameter by type and set custom value to it.</summary>
    /// <param name="source">Original parameters rules.</param>
    /// <param name="parameterType">The type of the parameter.</param>
    /// <param name="getCustomValue">Custom value provider.</param>
    /// <returns>New parameters rules.</returns>
    public static ParameterSelector Type(this ParameterSelector source,
        Type parameterType, Func<Request, object> getCustomValue) =>
        source.Details((r, p) => p.ParameterType == parameterType ? ServiceDetails.Of(getCustomValue(r)) : null);
  }

  /// <summary>DSL for specifying <see cref="PropertiesAndFieldsSelector"/> injection rules.</summary>
  public static partial class PropertiesAndFields
  {
    /// <summary>Say to not resolve any properties or fields.</summary>
    public static PropertiesAndFieldsSelector Of = request => null;

    /// <summary>Public assignable instance members of any type except object, string, primitives types, and arrays of those.</summary>
    public static PropertiesAndFieldsSelector Auto = All(withNonPublic: false, withPrimitive: false);

    /// <summary>Public, declared, assignable, non-primitive properties.</summary>
    public static PropertiesAndFieldsSelector Properties(
        bool withNonPublic = false, bool withBase = false,
        IfUnresolved ifUnresolved = IfUnresolved.ReturnDefaultIfNotRegistered) =>
        All(withNonPublic: withNonPublic, withPrimitive: false, withFields: false, withBase: withBase, ifUnresolved: ifUnresolved);

    /// <summary>Should return service info for input member (property or field).</summary>
    public delegate PropertyOrFieldServiceInfo GetServiceInfo(MemberInfo member, Request request);

    /// <summary>Generates selector property and field selector with settings specified by parameters.
    /// If all parameters are omitted the return all public not primitive members.</summary>
    public static PropertiesAndFieldsSelector All(
        bool withNonPublic = true,
        bool withPrimitive = true,
        bool withFields = true,
        bool withBase = true,
        IfUnresolved ifUnresolved = IfUnresolved.ReturnDefaultIfNotRegistered,
        GetServiceInfo serviceInfo = null)
    {
      GetServiceInfo info = (m, r) =>
          serviceInfo != null ? serviceInfo(m, r) :
          PropertyOrFieldServiceInfo.Of(m).WithDetails(ServiceDetails.Of(ifUnresolved: ifUnresolved));

      return req =>
      {
        var properties = req.ImplementationType.GetMembers(x => x.DeclaredProperties, includeBase: withBase) // todo: @perf optimize allocations 
            .Match(p => p.IsInjectable(withNonPublic, withPrimitive), p => info(p, req));

        if (!withFields)
          return properties;

        var fields = req.ImplementationType // todo: @perf optimize allocations and maybe combine with properties
            .GetMembers(x => x.DeclaredFields, includeBase: withBase)
            .Match(f => f.IsInjectable(withNonPublic, withPrimitive), f => info(f, req));

        return properties.Append(fields);
      };
    }

    /// <summary>Combines source properties and fields with other. Other will override the source condition.</summary>
    /// <param name="source">Source selector.</param> <param name="other">Specific other selector to add.</param>
    /// <returns>Combined result selector.</returns>
    public static PropertiesAndFieldsSelector OverrideWith(
        this PropertiesAndFieldsSelector source, PropertiesAndFieldsSelector other)
    {
      return source == null || source == Of ? (other ?? Of)
          : other == null || other == Of ? source
          : r =>
          {
            var sourceMembers = source(r).ToArrayOrSelf();
            var otherMembers = other(r).ToArrayOrSelf();
            return sourceMembers == null || sourceMembers.Length == 0 ? otherMembers
                      : otherMembers == null || otherMembers.Length == 0 ? sourceMembers
                      : otherMembers.Append(
                          sourceMembers.Match(otherMembers, (om, s) => s != null && om.All(o => o == null || !s.Member.Name.Equals(o.Member.Name))));
          };
    }

    /// <summary>Obsolete: please use <see cref="OverrideWith"/></summary>
    [Obsolete("Replaced with OverrideWith", false)]
    public static PropertiesAndFieldsSelector And(
        this PropertiesAndFieldsSelector source, PropertiesAndFieldsSelector other) =>
        source.OverrideWith(other);

    /// <summary>Specifies service details (key, if-unresolved policy, required type) for property/field with the name.</summary>
    /// <param name="source">Original member selector.</param> <param name="name">Member name.</param> <param name="getDetails">Details.</param>
    /// <returns>New selector.</returns>
    public static PropertiesAndFieldsSelector Details(this PropertiesAndFieldsSelector source,
        string name, Func<Request, ServiceDetails> getDetails)
    {
      name.ThrowIfNull();
      getDetails.ThrowIfNull();
      return source.OverrideWith(req =>
      {
        var implType = req.GetKnownImplementationOrServiceType();

        var property = implType
            .GetMembers(x => x.DeclaredProperties, includeBase: true)
            .FindFirst(x => x.Name == name);
        if (property != null && property.IsInjectable(true, true))
          return getDetails(req)?.To(PropertyOrFieldServiceInfo.Of(property).WithDetails).One();

        var field = implType
            .GetMembers(x => x.DeclaredFields, includeBase: true)
            .FindFirst(x => x.Name == name);
        if (field != null && field.IsInjectable(true, true))
          return getDetails(req)?.To(PropertyOrFieldServiceInfo.Of(field).WithDetails).One();

        return Throw.For<IEnumerable<PropertyOrFieldServiceInfo>>(
            Error.NotFoundSpecifiedWritablePropertyOrField, name, req);
      });
    }

    /// <summary>Adds to <paramref name="source"/> selector service info for property/field identified by <paramref name="name"/>.</summary>
    /// <param name="source">Source selector.</param> <param name="name">Name to identify member.</param>
    /// <param name="requiredServiceType">(optional)</param> <param name="serviceKey">(optional)</param>
    /// <param name="ifUnresolved">(optional) By default returns default value if unresolved.</param>
    /// <param name="defaultValue">(optional) Specifies default value to use when unresolved.</param>
    /// <param name="metadataKey">(optional) Required metadata key</param> <param name="metadata">Required metadata or value.</param>
    /// <returns>Combined selector.</returns>
    public static PropertiesAndFieldsSelector Name(this PropertiesAndFieldsSelector source, string name,
        Type requiredServiceType = null, object serviceKey = null,
        IfUnresolved ifUnresolved = IfUnresolved.ReturnDefault, object defaultValue = null,
        string metadataKey = null, object metadata = null) =>
        source.Details(name, r => ServiceDetails.Of(
            requiredServiceType, serviceKey, ifUnresolved, defaultValue, metadataKey, metadata));

    /// <summary>Specifies custom value for property/field with specific name.</summary>
    public static PropertiesAndFieldsSelector Name(this PropertiesAndFieldsSelector source,
        string name, Func<Request, object> getCustomValue) =>
        source.Details(name, r => ServiceDetails.Of(getCustomValue(r)));

    /// <summary>Returns true if property matches flags provided.</summary>
    /// <param name="property">Property to match</param>
    /// <param name="withNonPublic">Says to include non public properties.</param>
    /// <param name="withPrimitive">Says to include properties of primitive type.</param>
    /// <returns>True if property is matched and false otherwise.</returns>
    public static bool IsInjectable(this PropertyInfo property,
        bool withNonPublic = false, bool withPrimitive = false)
    {
      if (!property.CanWrite || property.IsExplicitlyImplemented())
        return false;

      if (property.IsStatic())
        return false;

      return !property.IsIndexer() &&
             (withNonPublic || property.GetSetMethodOrNull() != null) &&
             (withPrimitive || !property.PropertyType.IsPrimitive(orArrayOfPrimitives: true));
    }

    /// <summary>Returns true if field matches flags provided.</summary>
    /// <param name="field">Field to match.</param>
    /// <param name="withNonPublic">Says to include non public fields.</param>
    /// <param name="withPrimitive">Says to include fields of primitive type.</param>
    /// <returns>True if property is matched and false otherwise.</returns>
    public static bool IsInjectable(this FieldInfo field,
        bool withNonPublic = false, bool withPrimitive = false) =>
        !field.IsInitOnly && !field.IsBackingField()
            && (withNonPublic || field.IsPublic)
            && (withPrimitive || !field.FieldType.IsPrimitive(orArrayOfPrimitives: true));
  }

  /// <summary>Reflects on <see cref="ImplementationType"/> constructor parameters and members,
  /// creates expression for each reflected dependency, and composes result service expression.</summary>
  public sealed class ReflectionFactory : Factory
  {
    /// <summary>Non-abstract service implementation type. May be open generic.</summary>
    public override Type ImplementationType
    {
      get
      {
        if (_implementationType == null && _implementationTypeProvider != null)
          SetKnownImplementationType(_implementationTypeProvider(), Made);
        return _implementationType;
      }
    }

    /// <summary>False for lazy implementation type, to prevent its early materialization.</summary>
    public override bool CanAccessImplementationType =>
        _implementationType != null || _implementationTypeProvider == null;

    /// <summary>Provides closed-generic factory for registered open-generic variant.</summary>
    public override IConcreteFactoryGenerator FactoryGenerator => _factoryGenerator;

    /// <summary>Injection rules set for Constructor/FactoryMethod, Parameters, Properties and Fields.</summary>
    public override Made Made => _made;

    /// <summary>FactoryID of generator (open-generic) factory.</summary>
    public int GeneratorFactoryID { get; private set; }

    /// <summary>Will contain factory ID of generator's factory for generated factory.</summary>
    public override int RegistrationOrder => GeneratorFactoryID != 0 ? GeneratorFactoryID : FactoryID;

    /// <summary>Abstracts the factory construction, maybe optimized later without breaking the API</summary>
    public static ReflectionFactory Of(Type implementationType) => new ReflectionFactory(implementationType);

    /// <summary>Creates factory providing implementation type, optional reuse and setup.</summary>
    /// <param name="implementationType">(optional) Optional if Made.FactoryMethod is present Non-abstract close or open generic type.</param>
    /// <param name="reuse">(optional)</param> <param name="made">(optional)</param> <param name="setup">(optional)</param>
    public ReflectionFactory(Type implementationType = null, IReuse reuse = null, Made made = null, Setup setup = null)
        : base(reuse, setup)
    {
      _made = made ?? Made.Default;
      SetKnownImplementationType(implementationType, _made);
    }

    /// <summary>Creates factory providing implementation type, optional reuse and setup.</summary>
    /// <param name="implementationTypeProvider">Provider of non-abstract closed or open-generic type.</param>
    /// <param name="reuse">(optional)</param> <param name="made">(optional)</param> <param name="setup">(optional)</param>
    public ReflectionFactory(Func<Type> implementationTypeProvider, IReuse reuse = null, Made made = null, Setup setup = null)
        : base(reuse, setup)
    {
      _made = made ?? Made.Default;
      _implementationTypeProvider = implementationTypeProvider.ThrowIfNull();
    }

    /// <summary>Creates service expression.</summary>
    public override Expression CreateExpressionOrDefault(Request request)
    {
      var container = request.Container;
      var rules = container.Rules;

      var factoryMethodSelector = Made.FactoryMethod ?? rules.FactoryMethod;
      var factoryMethod = factoryMethodSelector?.Invoke(request);
      if (factoryMethod == null && factoryMethodSelector != null)
        return Throw.For<Expression>(request.IfUnresolved != IfUnresolved.ReturnDefault,
            Error.UnableToSelectCtor, request.ImplementationType, request);

      ConstructorInfo ctor;
      MethodBase ctorOrMethod;
      Expression factoryExpr = null;
      if (factoryMethod == null)
      {
        ctorOrMethod = ctor = _knownSingleCtor ?? request.ImplementationType.SingleConstructor();
      }
      else
      {
        // If factory method is the method of some registered service, then resolve factory service first.
        factoryExpr = factoryMethod.FactoryExpression;
        if (factoryExpr == null && factoryMethod.FactoryServiceInfo != null)
        {
          var factoryRequest = request.Push(factoryMethod.FactoryServiceInfo);
          factoryExpr = container.ResolveFactory(factoryRequest)?.GetExpressionOrDefault(factoryRequest);
          if (factoryExpr == null)
            return null; // todo: @check should we check for request.IfUnresolved != IfUnresolved.ReturnDefault here?
        }

        // return earlier if already have the parameters resolved, e.g. when using `ConstructorWithResolvableArguments`
        var ctorOrMember = factoryMethod.ConstructorOrMethodOrMember;
        if (factoryMethod.ResolvedParameterExpressions != null)
        {
          if (rules.UsedForValidation)
          {
            TryGetMemberAssignments(request, container, rules);
            return request.GetActualServiceType().GetDefaultValueExpression();
          }

          var newExpr = New((ConstructorInfo)ctorOrMember, factoryMethod.ResolvedParameterExpressions);
          var assignements = TryGetMemberAssignments(request, container, rules);
          return assignements == null ? newExpr : (Expression)MemberInit(newExpr, assignements);
        }

        ctorOrMethod = ctorOrMember as MethodBase;
        if (ctorOrMethod == null) // return earlier when factory is Property or Field
          return ConvertExpressionIfNeeded(ctorOrMember is PropertyInfo p ? Property(factoryExpr, p)
              : (Expression)Field(factoryExpr, (FieldInfo)ctorOrMember), request, ctorOrMember);

        ctor = ctorOrMember as ConstructorInfo;
      }

      var parameters = ctorOrMethod.GetParameters();
      if (parameters.Length == 0)
      {
        if (rules.UsedForValidation)
        {
          if (ctor != null)
            TryGetMemberAssignments(request, container, rules);
          return request.GetActualServiceType().GetDefaultValueExpression();
        }

        if (ctor == null)
          return ConvertExpressionIfNeeded(Call(factoryExpr, (MethodInfo)ctorOrMethod), request, ctorOrMethod);
        var assignements = TryGetMemberAssignments(request, container, rules);
        return assignements != null
            ? (Expression)MemberInit(New(ctor, Empty<Expression>()), assignements)
            : New(ctor, Empty<Expression>());
      }

      Expression arg0 = null, arg1 = null, arg2 = null, arg3 = null, arg4 = null;
      var paramExprs = parameters.Length > 5 ? new Expression[parameters.Length] : null;
      var paramSelector = rules.TryGetParameterSelector(Made)(request);

      var inputArgs = request.InputArgExprs;
      var argsUsedMask = 0;
      for (var i = 0; i < parameters.Length; i++)
      {
        var param = parameters[i];
        if (inputArgs != null)
        {
          var inputArgExpr = TryGetExpressionFromInputArgs(param.ParameterType, inputArgs, ref argsUsedMask);
          if (inputArgExpr != null)
          {
            if (paramExprs != null)
              paramExprs[i] = inputArgExpr;
            else if (i == 0)
              arg0 = inputArgExpr;
            else if (i == 1)
              arg1 = inputArgExpr;
            else if (i == 2)
              arg2 = inputArgExpr;
            else if (i == 3)
              arg3 = inputArgExpr;
            else
              arg4 = inputArgExpr;
            continue;
          }
        }

        var paramInfo = paramSelector(param) ?? ParameterServiceInfo.Of(param);
        var paramRequest = request.Push(paramInfo);
        var paramDetails = paramInfo.Details;
        var usedOrCustomValExpr = TryGetUsedInstanceOrCustomValueExpression(request, paramRequest, paramDetails);
        if (usedOrCustomValExpr != null)
        {
          if (paramExprs != null)
            paramExprs[i] = usedOrCustomValExpr;
          else if (i == 0)
            arg0 = usedOrCustomValExpr;
          else if (i == 1)
            arg1 = usedOrCustomValExpr;
          else if (i == 2)
            arg2 = usedOrCustomValExpr;
          else if (i == 3)
            arg3 = usedOrCustomValExpr;
          else
            arg4 = usedOrCustomValExpr;
          continue;
        }

        var injectedExpr = container.ResolveFactory(paramRequest)?.GetExpressionOrDefault(paramRequest);
        if (injectedExpr == null ||
            // When param is an empty array / collection, then we may use a default value instead (#581)
            paramDetails.DefaultValue != null &&
            injectedExpr.NodeType == System.Linq.Expressions.ExpressionType.NewArrayInit &&
            ((NewArrayExpression)injectedExpr).Expressions.Count == 0)
        {
          // Check if parameter dependency itself (without propagated parent details)
          // does not allow default, then stop checking the rest of parameters.
          if (paramDetails.IfUnresolved == IfUnresolved.Throw)
            return null;
          injectedExpr = paramDetails.DefaultValue != null
              ? container.GetConstantExpression(paramDetails.DefaultValue)
              : paramRequest.ServiceType.GetDefaultValueExpression();
        }

        if (paramExprs != null)
          paramExprs[i] = injectedExpr;
        else if (i == 0)
          arg0 = injectedExpr;
        else if (i == 1)
          arg1 = injectedExpr;
        else if (i == 2)
          arg2 = injectedExpr;
        else if (i == 3)
          arg3 = injectedExpr;
        else
          arg4 = injectedExpr;
      }

      if (rules.UsedForValidation)
        return request.GetActualServiceType().GetDefaultValueExpression();

      Expression serviceExpr;
      if (arg0 == null)
        serviceExpr = ctor != null ? New(ctor, paramExprs) : (Expression)Call(factoryExpr, (MethodInfo)ctorOrMethod, paramExprs);
      else if (arg1 == null)
        serviceExpr = ctor != null ? New(ctor, arg0) : (Expression)Call(factoryExpr, (MethodInfo)ctorOrMethod, arg0);
      else if (arg2 == null)
        serviceExpr = ctor != null ? New(ctor, arg0, arg1) : (Expression)Call(factoryExpr, (MethodInfo)ctorOrMethod, arg0, arg1);
      else if (arg3 == null)
        serviceExpr = ctor != null ? New(ctor, arg0, arg1, arg2) : (Expression)Call(factoryExpr, (MethodInfo)ctorOrMethod, arg0, arg1, arg2);
      else if (arg4 == null)
        serviceExpr = ctor != null ? New(ctor, arg0, arg1, arg2, arg3) : (Expression)Call(factoryExpr, (MethodInfo)ctorOrMethod, arg0, arg1, arg2, arg3);
      else
        serviceExpr = ctor != null ? New(ctor, arg0, arg1, arg2, arg3, arg4) : (Expression)Call(factoryExpr, (MethodInfo)ctorOrMethod, arg0, arg1, arg2, arg3, arg4);

      if (ctor == null)
        return ConvertExpressionIfNeeded(serviceExpr, request, ctorOrMethod);

      var assignments = TryGetMemberAssignments(request, container, rules);
      if (assignments == null)
        return serviceExpr;

      return MemberInit((NewExpression)serviceExpr, assignments);
    }

    private MemberAssignment[] TryGetMemberAssignments(Request request, IContainer container, Rules rules)
    {
      if (rules.PropertiesAndFields == null && Made.PropertiesAndFields == null)
        return null;

      var propertiesAndFields = rules.TryGetPropertiesAndFieldsSelector(Made).Invoke(request);
      if (propertiesAndFields == null)
        return null;

      MemberAssignment[] assignments = null;
      foreach (var member in propertiesAndFields)
        if (member != null)
        {
          var memberRequest = request.Push(member);
          var memberExpr =
              TryGetUsedInstanceOrCustomValueExpression(request, memberRequest, member.Details)
              ?? container.ResolveFactory(memberRequest)?.GetExpressionOrDefault(memberRequest);
          if (memberExpr != null)
            assignments = assignments.Append(Bind(member.Member, memberExpr));
          else if (request.IfUnresolved == IfUnresolved.ReturnDefault)
            return null;
        }

      return assignments;
    }

    private static Expression ConvertExpressionIfNeeded(Expression serviceExpr, Request request, MemberInfo ctorOrMember)
    {
      var actualServiceType = request.GetActualServiceType();
      var serviceExprType = serviceExpr.Type;
      return serviceExprType == actualServiceType || actualServiceType.GetTypeInfo().IsAssignableFrom(serviceExprType.GetTypeInfo())
              ? serviceExpr
          : serviceExprType == typeof(object)
              ? Convert(serviceExpr, actualServiceType)
          : serviceExprType.HasConversionOperatorTo(actualServiceType)
              ? Convert(serviceExpr, actualServiceType)
          : request.IfUnresolved != IfUnresolved.Throw
                  ? null
                  : Throw.For<Expression>(Error.ServiceIsNotAssignableFromFactoryMethod, actualServiceType, ctorOrMember,
                      request);
    }

    // Check not yet used arguments provided via `Func<Arg, TService>` or `Resolve(.., args: new[] { arg })`
    internal static Expression TryGetExpressionFromInputArgs(Type paramType, Expression[] inputArgs, ref int argsUsedMask)
    {
      for (var a = 0; a < inputArgs.Length; ++a)
        if ((argsUsedMask & 1 << a) == 0 && inputArgs[a].Type.IsAssignableTo(paramType))
        {
          argsUsedMask |= 1 << a; // mark that argument was used
          return inputArgs[a];
        }
      return null;
    }

    internal static Expression TryGetUsedInstanceOrCustomValueExpression(Request request, Request paramRequest, ServiceDetails paramDetails)
    {
      if (paramDetails.HasCustomValue)
      {
        var serviceType = paramRequest.ServiceType;
        var hasConversionOperator = false;
        var customValue = paramDetails.CustomValue;
        if (customValue != null)
        {
          var customTypeValue = customValue.GetType();
          if (!customTypeValue.IsArray &&
              !customTypeValue.IsAssignableTo(serviceType) &&
              !(hasConversionOperator = customTypeValue.HasConversionOperatorTo(serviceType)))
            return Throw.For<Expression>(paramRequest.IfUnresolved != IfUnresolved.ReturnDefault,
                Error.InjectedCustomValueIsOfDifferentType, customValue, serviceType, paramRequest);
        }

        return hasConversionOperator
            ? Convert(request.Container.GetConstantExpression(customValue), serviceType)
            : request.Container.GetConstantExpression(customValue, serviceType);
      }

      if (paramDetails == ServiceDetails.Default)
      {
        // Generate the fast resolve call for used instances
        if (request.Container.TryGetUsedInstance(paramRequest.ServiceType, out var instance))
          return Call(ResolverContext.GetRootOrSelfExpr(paramRequest), Resolver.ResolveFastMethod,
              Constant(paramRequest.ServiceType, typeof(Type)), Constant(paramRequest.IfUnresolved));
      }

      return null;
    }

    internal override bool ValidateAndNormalizeRegistration(Type serviceType, object serviceKey, bool isStaticallyChecked, Rules rules)
    {
      base.ValidateAndNormalizeRegistration(serviceType, serviceKey, isStaticallyChecked, rules);

      if (!CanAccessImplementationType)
        return true;

      var implType = ImplementationType;
      if (Made.FactoryMethod == null && rules.FactoryMethod == null)
      {
        var ctors = implType.GetTypeInfo().DeclaredConstructors.ToArrayOrSelf();
        var ctorCount = 0;
        for (var i = 0; ctorCount != 2 && i < ctors.Length; i++)
        {
          var ctor = ctors[i];
          if (ctor.IsPublic && !ctor.IsStatic)
          {
            ++ctorCount;
            _knownSingleCtor = ctor;
          }
        }

        if (ctorCount == 0)
          Throw.It(Error.UnableToSelectSinglePublicConstructorFromNone, implType);
        else if (ctorCount > 1)
          Throw.It(Error.UnableToSelectSinglePublicConstructorFromMultiple, implType, ctors);
      }

      if (isStaticallyChecked || implType == null)
        return true;

      var implTypeInfo = implType.GetTypeInfo();
      if (!implTypeInfo.IsGenericTypeDefinition)
      {
        if (implTypeInfo.IsGenericType && implTypeInfo.ContainsGenericParameters)
          Throw.It(Error.RegisteringNotAGenericTypedefImplType, implType, implType.GetGenericTypeDefinition());

        else if (implType != serviceType && serviceType != typeof(object))
        {
          var serviceTypeInfo = serviceType.GetTypeInfo();
          if (!serviceTypeInfo.IsGenericTypeDefinition)
          {
            if (!serviceTypeInfo.IsAssignableFrom(implTypeInfo))
              Throw.It(Error.RegisteringImplementationNotAssignableToServiceType, implType, serviceType);
          }
          else
          {
            if (implType.GetImplementedTypes().IndexOf(serviceType, (st, t) => t == st || t.GetGenericDefinitionOrNull() == st) == -1)
              Throw.It(Error.RegisteringImplementationNotAssignableToServiceType, implType, serviceType);
          }
        }
      }
      else if (implType != serviceType)
      {
        if (serviceType.IsGenericDefinition())
          ThrowIfImplementationAndServiceTypeParamsDontMatch(implType, serviceType);

        else if (!serviceType.IsGeneric())
          Throw.It(Error.RegisteringOpenGenericImplWithNonGenericService, implType, serviceType);

        else if (!implType.IsImplementingServiceType(serviceType.GetGenericTypeDefinition()))
          Throw.It(Error.RegisteringImplementationNotAssignableToServiceType, implType, serviceType);
      }

      return true;
    }

    private static void ThrowIfImplementationAndServiceTypeParamsDontMatch(Type implType, Type serviceType)
    {
      var implTypeParams = implType.GetGenericParamsAndArgs();
      var implementedTypes = implType.GetImplementedTypes();

      var implementedTypeFound = false;
      var containsAllTypeParams = false;
      for (var i = 0; !containsAllTypeParams && i < implementedTypes.Length; ++i)
      {
        var implementedType = implementedTypes[i];
        implementedTypeFound = implementedType.GetGenericDefinitionOrNull() == serviceType;
        containsAllTypeParams = implementedTypeFound
            && implementedType.ContainsAllGenericTypeParameters(implTypeParams);
      }

      if (!implementedTypeFound)
        Throw.It(Error.RegisteringImplementationNotAssignableToServiceType, implType, serviceType);

      if (!containsAllTypeParams)
        Throw.It(Error.RegisteringOpenGenericServiceWithMissingTypeArgs,
            implType, serviceType,
            implementedTypes.Where(t => t.GetGenericDefinitionOrNull() == serviceType));
    }

    #region Implementation

    private Type _implementationType; // non-readonly to be set by lazy type provider
    private readonly Func<Type> _implementationTypeProvider;
    private readonly Made _made;
    private ClosedGenericFactoryGenerator _factoryGenerator;
    private ConstructorInfo _knownSingleCtor;

    private sealed class ClosedGenericFactoryGenerator : IConcreteFactoryGenerator
    {
      public ImHashMap<KV<Type, object>, ReflectionFactory> GeneratedFactories => _generatedFactories.Value;

      public ClosedGenericFactoryGenerator(ReflectionFactory openGenericFactory)
      {
        _openGenericFactory = openGenericFactory;
      }

      // todo: @perf optimize request.Details access and reflection here
      public Factory GetGeneratedFactory(Request request, bool ifErrorReturnDefault = false)
      {
        var openFactory = _openGenericFactory;
        var implType = openFactory._implementationType;
        var serviceType = request.GetActualServiceType();

        var closedTypeArgs = implType == null || implType == serviceType.GetGenericDefinitionOrNull()
          ? serviceType.GetGenericParamsAndArgs()
          : implType.IsGenericParameter ? serviceType.One()
          : GetClosedTypeArgsOrNullForOpenGenericType(implType, serviceType, request, ifErrorReturnDefault);

        if (closedTypeArgs == null)
          return null;

        var made = openFactory.Made;
        if (made.FactoryMethod != null)
        {
          // resolve request with factory to specify the implementation type may be required by FactoryMethod or GetClosed...
          request = request.WithResolvedFactory(openFactory, ifErrorReturnDefault, ifErrorReturnDefault, copyRequest: true);
          var factoryMethod = made.FactoryMethod(request);
          if (factoryMethod == null)
            return ifErrorReturnDefault ? null : Throw.For<Factory>(Error.GotNullFactoryWhenResolvingService, request);

          var checkMatchingType = implType != null && implType.IsGenericParameter;
          var closedFactoryMethod = GetClosedFactoryMethodOrDefault(factoryMethod, closedTypeArgs, request, checkMatchingType);

          // may be null only for `IfUnresolved.ReturnDefault` or if the check for matching type is failed
          if (closedFactoryMethod == null)
            return null;

          made = Made.Of(closedFactoryMethod, made.Parameters, made.PropertiesAndFields);
        }

        var details = request._serviceInfo.Details;
        if (implType != null)
        {
          implType = implType.IsGenericParameter
              ? closedTypeArgs[0]
              : implType.TryCloseGenericTypeOrMethod(closedTypeArgs, (t, a) => t.MakeGenericType(a),
                  !ifErrorReturnDefault && details.IfUnresolved == IfUnresolved.Throw, Error.NoMatchedGenericParamConstraints, request);
          if (implType == null)
            return null;
        }

        var knownImplOrServiceType = implType ?? made.FactoryMethodKnownResultType ?? serviceType;
        var serviceKey = details.ServiceKey;
        serviceKey = (serviceKey as OpenGenericTypeKey)?.ServiceKey ?? serviceKey ?? DefaultKey.Value;
        var generatedFactoryKey = KV.Of(knownImplOrServiceType, serviceKey);

        var generatedFactories = _generatedFactories.Value;
        if (!generatedFactories.IsEmpty)
        {
          var generatedFactory = generatedFactories.GetValueOrDefault(generatedFactoryKey);
          if (generatedFactory != null)
            return generatedFactory;
        }

        var closedGenericFactory = new ReflectionFactory(implType, openFactory.Reuse, made, openFactory.Setup)
        {
          GeneratorFactoryID = openFactory.FactoryID,
          Caching = openFactory.Caching
        };

        _generatedFactories.Swap(generatedFactoryKey, closedGenericFactory,
            (x, genFacKey, closedGenFac) => x.AddOrUpdate(genFacKey, closedGenFac,
            (oldFac, _) => closedGenericFactory = oldFac));

        return closedGenericFactory;
      }

      private readonly ReflectionFactory _openGenericFactory;
      private readonly Ref<ImHashMap<KV<Type, object>, ReflectionFactory>>
          _generatedFactories = Ref.Of(ImHashMap<KV<Type, object>, ReflectionFactory>.Empty);
    }

    private void SetKnownImplementationType(Type implType, Made made)
    {
      var knownImplType = implType;

      var factoryMethodResultType = Made.FactoryMethodKnownResultType;
      if (implType == null ||
          implType == typeof(object) || // required as currently object represents the open-generic type argument T registrations
          implType.IsAbstract())
      {
        if (made.FactoryMethod == null)
        {
          if (implType == null)
            Throw.It(Error.RegisteringNullImplementationTypeAndNoFactoryMethod);
          if (implType == typeof(object))
            Throw.It(Error.RegisteringObjectTypeAsImplementationIsNotSupported);
          if (implType.IsAbstract())
            Throw.It(Error.RegisteringAbstractImplementationTypeAndNoFactoryMethod, implType);
        }

        knownImplType = null; // Ensure that we do not have abstract implementation type

        // Using non-abstract factory method result type is safe for conditions and diagnostics
        if (factoryMethodResultType != null &&
            factoryMethodResultType != typeof(object) &&
            !factoryMethodResultType.IsAbstract())
          knownImplType = factoryMethodResultType;
      }
      else if (factoryMethodResultType != null
            && factoryMethodResultType != implType)
      {
        if (!factoryMethodResultType.IsAssignableTo(implType) &&
            !factoryMethodResultType.HasConversionOperatorTo(implType))
          Throw.It(Error.RegisteredFactoryMethodResultTypesIsNotAssignableToImplementationType,
              implType, factoryMethodResultType);
      }

      var openGenericImplType = knownImplType ?? implType;
      if (openGenericImplType == typeof(object) || // for open-generic T implementation
          openGenericImplType != null &&           // for open-generic X<T> implementation
          (openGenericImplType.IsGenericDefinition() || openGenericImplType.IsGenericParameter) ||
          made.IsConditionalImplementation)
      {
        _factoryGenerator = new ClosedGenericFactoryGenerator(this);
      }

      _implementationType = knownImplType;
    }

    private static Type[] GetClosedTypeArgsOrNullForOpenGenericType(
        Type openImplType, Type closedServiceType, Request request, bool ifErrorReturnDefault)
    {
      var serviceTypeArgs = closedServiceType.GetGenericParamsAndArgs();
      var serviceTypeGenericDef = closedServiceType.GetGenericTypeDefinition();

      var implTypeParams = openImplType.GetGenericParamsAndArgs();
      var implTypeArgs = new Type[implTypeParams.Length];

      var implementedTypes = openImplType.GetImplementedTypes();

      var matchFound = false;
      for (var i = 0; !matchFound && i < implementedTypes.Length; ++i)
      {
        var implementedType = implementedTypes[i];
        if (implementedType.IsOpenGeneric() && implementedType.GetGenericDefinitionOrNull() == serviceTypeGenericDef)
          matchFound = MatchServiceWithImplementedTypeParams(
              implTypeArgs, implTypeParams, implementedType.GetGenericParamsAndArgs(), serviceTypeArgs);
      }

      if (!matchFound)
        return ifErrorReturnDefault || request.IfUnresolved != IfUnresolved.Throw ? null
            : Throw.For<Type[]>(Error.NoMatchedImplementedTypesWithServiceType,
                openImplType, implementedTypes, request);

      MatchOpenGenericConstraints(implTypeParams, implTypeArgs);

      var notMatchedIndex = Array.IndexOf(implTypeArgs, null);
      if (notMatchedIndex != -1)
        return ifErrorReturnDefault || request.IfUnresolved != IfUnresolved.Throw ? null
            : Throw.For<Type[]>(Error.NotFoundOpenGenericImplTypeArgInService,
                openImplType, implTypeParams[notMatchedIndex], request);

      return implTypeArgs;
    }

    private static void MatchOpenGenericConstraints(Type[] implTypeParams, Type[] implTypeArgs)
    {
      for (var i = 0; i < implTypeParams.Length; i++)
      {
        var implTypeArg = implTypeArgs[i];
        if (implTypeArg == null)
          continue; // skip yet unknown type arg

        var implTypeParamConstraints = implTypeParams[i].GetGenericParamConstraints();
        if (implTypeParamConstraints.IsNullOrEmpty())
          continue; // skip case with no constraints

        // match type parameters inside constraint
        var constraintMatchFound = false;
        for (var j = 0; !constraintMatchFound && j < implTypeParamConstraints.Length; ++j)
        {
          var implTypeParamConstraint = implTypeParamConstraints[j];
          if (implTypeParamConstraint != implTypeArg && implTypeParamConstraint.IsOpenGeneric())
          {
            var implTypeArgArgs = implTypeArg.IsGeneric() ? implTypeArg.GetGenericParamsAndArgs() : implTypeArg.One();
            var implTypeParamConstraintParams = implTypeParamConstraint.GetGenericParamsAndArgs();

            constraintMatchFound = MatchServiceWithImplementedTypeParams(
                implTypeArgs, implTypeParams, implTypeParamConstraintParams, implTypeArgArgs);
          }
        }
      }
    }

    private static bool MatchServiceWithImplementedTypeParams(
        Type[] resultImplArgs, Type[] implParams, Type[] serviceParams, Type[] serviceArgs,
        int resultCount = 0)
    {
      if (serviceArgs.Length != serviceParams.Length)
        return false;

      for (var i = 0; i < serviceParams.Length; i++)
      {
        var serviceArg = serviceArgs[i];
        var implementedParam = serviceParams[i];
        if (implementedParam.IsGenericParameter)
        {
          var paramIndex = implParams.Length - 1;
          while (paramIndex != -1 && !ReferenceEquals(implParams[paramIndex], implementedParam))
            --paramIndex;
          if (paramIndex != -1)
          {
            if (resultImplArgs[paramIndex] == null)
              resultImplArgs[paramIndex] = serviceArg;
            else if (resultImplArgs[paramIndex] != serviceArg)
              return false; // more than one service type arg is matching with single implementation type parameter
          }
        }
        else if (implementedParam != serviceArg)
        {
          if (!implementedParam.IsOpenGeneric() ||
              implementedParam.GetGenericDefinitionOrNull() != serviceArg.GetGenericDefinitionOrNull())
            return false; // type parameter and argument are of different types

          if (!MatchServiceWithImplementedTypeParams(resultImplArgs, implParams,
              implementedParam.GetGenericParamsAndArgs(), serviceArg.GetGenericParamsAndArgs()))
            return false; // nested match failed due one of above reasons.
        }
      }

      return true;
    }

    private static FactoryMethod GetClosedFactoryMethodOrDefault(
        FactoryMethod factoryMethod, Type[] serviceTypeArgs, Request request, bool ifErrorReturnDefault = false)
    {
      var factoryMember = factoryMethod.ConstructorOrMethodOrMember;
      var factoryInfo = factoryMethod.FactoryServiceInfo;

      var resultType = factoryMember.GetReturnTypeOrDefault();
      var implTypeParams = resultType.IsGenericParameter ? resultType.One() : resultType.GetGenericParamsAndArgs();

      // Get method declaring type, and if its open-generic,
      // then close it first. It is required to get actual method.
      var factoryImplType = factoryMember.DeclaringType.ThrowIfNull();
      if (factoryImplType.IsOpenGeneric())
      {
        var factoryImplTypeParams = factoryImplType.GetGenericParamsAndArgs();
        var resultFactoryImplTypeArgs = new Type[factoryImplTypeParams.Length];

        var isFactoryImplTypeClosed = MatchServiceWithImplementedTypeParams(
            resultFactoryImplTypeArgs, factoryImplTypeParams, implTypeParams, serviceTypeArgs);

        if (!isFactoryImplTypeClosed)
          return ifErrorReturnDefault || request.IfUnresolved != IfUnresolved.Throw ? null
              : Throw.For<FactoryMethod>(Error.NoMatchedFactoryMethodDeclaringTypeWithServiceTypeArgs,
                  factoryImplType, new StringBuilder().Print(serviceTypeArgs, itemSeparator: ", "), request);

        // For instance factory match its service type from the implementation factory type.
        if (factoryInfo != null)
        {
          // Look for service type equivalent within factory implementation type base classes and interfaces,
          // because we need identical type arguments to match.
          var factoryServiceType = factoryInfo.ServiceType;
          if (factoryServiceType != factoryImplType)
            factoryServiceType = factoryImplType.GetImplementedTypes()
                .FindFirst(factoryServiceType, (fServiceType, t) => t.IsGeneric() && t.GetGenericTypeDefinition() == fServiceType)
                .ThrowIfNull();

          var factoryServiceTypeParams = factoryServiceType.GetGenericParamsAndArgs();
          var resultFactoryServiceTypeArgs = new Type[factoryServiceTypeParams.Length];

          var isFactoryServiceTypeClosed = MatchServiceWithImplementedTypeParams(
              resultFactoryServiceTypeArgs, factoryServiceTypeParams, factoryImplTypeParams, resultFactoryImplTypeArgs);

          // Replace factory info with close factory service type
          if (isFactoryServiceTypeClosed)
          {
            factoryServiceType = factoryServiceType.GetGenericTypeDefinition().ThrowIfNull();
            var closedFactoryServiceType = factoryServiceType.TryCloseGenericTypeOrMethod(resultFactoryServiceTypeArgs, (t, a) => t.MakeGenericType(a),
                !ifErrorReturnDefault && request.IfUnresolved == IfUnresolved.Throw, Error.NoMatchedGenericParamConstraints, request);

            if (closedFactoryServiceType == null)
              return null;

            // Copy factory info with closed factory type
            factoryInfo = ServiceInfo.Of(closedFactoryServiceType).WithDetails(factoryInfo.Details);
          }
        }

        MatchOpenGenericConstraints(factoryImplTypeParams, resultFactoryImplTypeArgs);

        // Close the factory type implementation
        // and get factory member to use from it.
        var closedFactoryImplType = factoryImplType.TryCloseGenericTypeOrMethod(resultFactoryImplTypeArgs, (t, a) => t.MakeGenericType(a),
            !ifErrorReturnDefault && request.IfUnresolved == IfUnresolved.Throw, Error.NoMatchedGenericParamConstraints, request);

        if (closedFactoryImplType == null)
          return null;

        // Find corresponding member again, now from closed type
        var factoryMethodBase = factoryMember as MethodBase;
        if (factoryMethodBase != null)
        {
          var factoryMethodParameters = factoryMethodBase.GetParameters();
          var targetMethods = closedFactoryImplType.GetMembers(t => t.DeclaredMethods, includeBase: true)
              .Match(m => m.Name == factoryMember.Name && m.GetParameters().Length == factoryMethodParameters.Length)
              .ToArrayOrSelf();

          if (targetMethods.Length == 1)
            factoryMember = targetMethods[0];
          else // Fallback to MethodHandle only if methods have similar signatures
          {
            var methodHandleProperty = typeof(MethodBase).GetTypeInfo()
                .DeclaredProperties
                .FindFirst(it => it.Name == "MethodHandle")
                .ThrowIfNull(Error.OpenGenericFactoryMethodDeclaringTypeIsNotSupportedOnThisPlatform,
                    factoryImplType, closedFactoryImplType, factoryMethodBase.Name);
            factoryMember = MethodBase.GetMethodFromHandle(
                (RuntimeMethodHandle)methodHandleProperty.GetValue(factoryMethodBase, Empty<object>()),
                closedFactoryImplType.TypeHandle);
          }
        }
        else if (factoryMember is FieldInfo)
        {
          factoryMember = closedFactoryImplType.GetMembers(t => t.DeclaredFields, includeBase: true)
              .Single(f => f.Name == factoryMember.Name);
        }
        else if (factoryMember is PropertyInfo)
        {
          factoryMember = closedFactoryImplType.GetMembers(t => t.DeclaredProperties, includeBase: true)
              .Single(f => f.Name == factoryMember.Name);
        }
      }

      // If factory method is actual method and still open-generic after closing its declaring type,
      // then match remaining method type parameters and make closed method
      var openFactoryMethod = factoryMember as MethodInfo;
      if (openFactoryMethod != null && openFactoryMethod.ContainsGenericParameters)
      {
        var methodTypeParams = openFactoryMethod.GetGenericArguments();
        var resultMethodTypeArgs = new Type[methodTypeParams.Length];

        var isMethodClosed = MatchServiceWithImplementedTypeParams(
            resultMethodTypeArgs, methodTypeParams, implTypeParams, serviceTypeArgs);

        if (!isMethodClosed)
          return ifErrorReturnDefault || request.IfUnresolved != IfUnresolved.Throw ? null
              : Throw.For<FactoryMethod>(Error.NoMatchedFactoryMethodWithServiceTypeArgs,
                  openFactoryMethod, new StringBuilder().Print(serviceTypeArgs, itemSeparator: ", "),
                  request);

        MatchOpenGenericConstraints(methodTypeParams, resultMethodTypeArgs);

        factoryMember = openFactoryMethod.TryCloseGenericTypeOrMethod(resultMethodTypeArgs, (m, a) => m.MakeGenericMethod(a),
            !ifErrorReturnDefault && request.IfUnresolved == IfUnresolved.Throw, Error.NoMatchedGenericParamConstraints, request);

        if (factoryMember == null)
          return null;
      }

      var factoryInstance = factoryMethod.FactoryExpression;
      return factoryInstance != null
          ? new FactoryMethod(factoryMember, factoryInstance)
          : new FactoryMethod(factoryMember, factoryInfo);
    }

    #endregion
  }

  /// <summary>Creates service expression using client provided expression factory delegate.</summary>
  public sealed class ExpressionFactory : Factory
  {
    /// <summary>Wraps provided delegate into factory.</summary>
    /// <param name="getServiceExpression">Delegate that will be used internally to create service expression.</param>
    /// <param name="reuse">(optional) Reuse.</param> <param name="setup">(optional) Setup.</param>
    public ExpressionFactory(Func<Request, Expression> getServiceExpression, IReuse reuse = null, Setup setup = null)
        : base(reuse, setup)
    {
      _getServiceExpression = getServiceExpression.ThrowIfNull();
    }

    /// <summary>Creates service expression using wrapped delegate.</summary>
    /// <param name="request">Request to resolve.</param> <returns>Expression returned by stored delegate.</returns>
    public override Expression CreateExpressionOrDefault(Request request) =>
        _getServiceExpression(request);

    private readonly Func<Request, Expression> _getServiceExpression;
  }

  /// Wraps the instance in registry
  public sealed class RegisteredInstanceFactory : Factory
  {
    /// <summary>The registered pre-created object instance</summary>
    public readonly object Instance;

    /// <summary>Non-abstract closed implementation type.</summary>
    public override Type ImplementationType { get; }

    /// <inheritdoc />
    public override bool HasRuntimeState => true;

    /// Simplified specially for register instance 
    internal override bool ValidateAndNormalizeRegistration(Type serviceType, object serviceKey, bool isStaticallyChecked, Rules rules)
    {
      if (!isStaticallyChecked && (ImplementationType != null && !ImplementationType.IsAssignableTo(serviceType.ThrowIfNull())))
        Throw.It(Error.RegisteringInstanceNotAssignableToServiceType, ImplementationType, serviceType);
      return true;
    }

    /// <summary>Creates factory.</summary>
    public RegisteredInstanceFactory(object instance, IReuse reuse = null, Setup setup = null)
       : base(reuse ?? Crystal.Reuse.Singleton,
           (setup ?? Setup.Default).WithAsResolutionCallForGeneratedExpression())
    {
      if (instance != null) // it may be `null` as well
      {
        ImplementationType = instance.GetType();
        if (Setup.WeaklyReferenced)
          Instance = new WeakReference(instance);
        else
          Instance = instance;
      }
    }

    /// <summary>Wraps the instance in expression constant</summary>
    public override Expression CreateExpressionOrDefault(Request request)
    {
      // unpacks the weak-reference
      if (Setup.WeaklyReferenced)
        return Call(
            typeof(ThrowInGeneratedCode).GetTypeInfo()
                .GetDeclaredMethod(nameof(ThrowInGeneratedCode.WeakRefReuseWrapperGCed)),
            Property(
                Constant(Instance, typeof(WeakReference)),
                typeof(WeakReference).Property(nameof(WeakReference.Target))));

      // otherwise just return a constant
      var instanceExpr = request.Container.GetConstantExpression(Instance);
      var serviceType = request.GetActualServiceType();
      if (serviceType.GetTypeInfo().IsAssignableFrom(ImplementationType.GetTypeInfo()))
        return instanceExpr;
      return Convert(instanceExpr, serviceType);
    }

    /// <summary>Simplified path for the registered instance</summary>
    public override Expression GetExpressionOrDefault(Request request)
    {
      if (// preventing recursion
          (request.Flags & RequestFlags.IsGeneratedResolutionDependencyExpression) == 0 && !request.IsResolutionCall &&
           (Setup.AsResolutionCall || Setup.AsResolutionCallForExpressionGeneration && request.Rules.UsedForExpressionGeneration))
        return Resolver.CreateResolutionExpression(request.WithResolvedFactory(this), Setup.OpenResolutionScope, Setup.AsResolutionCall);

      // First look for decorators if it is not already a decorator
      var serviceType = request.ServiceType;
      var serviceTypeInfo = serviceType.GetTypeInfo();
      if (serviceTypeInfo.IsArray)
        serviceType = typeof(IEnumerable<>).MakeGenericType(serviceTypeInfo.GetElementType());

      // todo: @perf Prevents from costly `WithResolvedFactory` call
      // todo: @hack with IContainer cast - move to the interface
      var decorators = ((Container)request.Container)._registry.Value.Decorators;
      if (!decorators.IsEmpty)
      {
        // todo: @perf optimize WithResolvedFactory for registered instance
        var decoratorExpr = request.Container.GetDecoratorExpressionOrDefault(request.WithResolvedFactory(this));
        if (decoratorExpr != null)
          return decoratorExpr;
      }

      return CreateExpressionOrDefault(request);
    }

    /// <summary>Used at resolution root too simplify getting the actual instance</summary>
    public override FactoryDelegate GetDelegateOrDefault(Request request)
    {
      request = request.WithResolvedFactory(this);

      if (request.Container.GetDecoratorExpressionOrDefault(request) != null)
        return base.GetDelegateOrDefault(request);

      return Setup.WeaklyReferenced
          ? (FactoryDelegate)UnpackWeakRefFactory
          : InstanceFactory;
    }

    private object InstanceFactory(IResolverContext _) => Instance;
    private object UnpackWeakRefFactory(IResolverContext _) => (Instance as WeakReference)?.Target.WeakRefReuseWrapperGCed();
  }

  /// <summary>This factory is the thin wrapper for user provided delegate
  /// and where possible it uses delegate directly: without converting it to expression.</summary>
  public sealed class DelegateFactory : Factory
  {
    /// <summary>Non-abstract closed implementation type.</summary>
    public override Type ImplementationType { get; }

    /// <inheritdoc />
    public override bool HasRuntimeState => true;

    /// <summary>Creates factory.</summary>
    public DelegateFactory(FactoryDelegate factoryDelegate,
       IReuse reuse = null, Setup setup = null, Type knownImplementationType = null)
       : base(reuse, (setup ?? Setup.Default).WithAsResolutionCallForGeneratedExpression())
    {
      _factoryDelegate = factoryDelegate.ThrowIfNull();
      ImplementationType = knownImplementationType;
    }

    /// <summary>Create expression by wrapping call to stored delegate with provided request.</summary>
    public override Expression CreateExpressionOrDefault(Request request)
    {
      // GetConstant here is needed to check the runtime state rule
      var delegateExpr = request.Container.GetConstantExpression(_factoryDelegate);
      var resolverExpr = ResolverContext.GetRootOrSelfExpr(request);
      return Convert(Invoke(delegateExpr, resolverExpr), request.GetActualServiceType());
    }

    /// <summary>If possible returns delegate directly, without creating expression trees, just wrapped in <see cref="FactoryDelegate"/>.
    /// If decorator found for request then factory fall-backs to expression creation.</summary>
    /// <param name="request">Request to resolve.</param>
    /// <returns>Factory delegate directly calling wrapped delegate, or invoking expression if decorated.</returns>
    public override FactoryDelegate GetDelegateOrDefault(Request request)
    {
      request = request.WithResolvedFactory(this);

      // Wrap the delegate in respective expression for non-simple use
      if (request.Reuse != Crystal.Reuse.Transient ||
          FactoryType == FactoryType.Service &&
          request.Container.GetDecoratorExpressionOrDefault(request) != null)
        return base.GetDelegateOrDefault(request);

      // Otherwise just use delegate as-is
      return _factoryDelegate;
    }

    private readonly FactoryDelegate _factoryDelegate;
  }

  internal sealed class FactoryPlaceholder : Factory
  {
    public static readonly Factory Default = new FactoryPlaceholder();

    // Always resolved asResolutionCall, to create a hole in object graph to be filled in later
    public override Setup Setup => _setup;
    private static readonly Setup _setup = Setup.With(asResolutionCall: true);

    public override Expression CreateExpressionOrDefault(Request request) =>
        Throw.For<Expression>(Error.NoImplementationForPlaceholder, request);
  }

  /// Should return value stored in scope
  public delegate object CreateScopedValue();

  /// <summary>Lazy object storage that will create object with provided factory on first access,
  /// then will be returning the same object for subsequent access.</summary>
  public interface IScope : IEnumerable<IScope>, IDisposable
  {
    /// <summary>Parent scope in scope stack. Null for root scope.</summary>
    IScope Parent { get; }

    /// <summary>Optional name object associated with scope.</summary>
    object Name { get; }

    /// <summary>True if scope is disposed.</summary>
    bool IsDisposed { get; }

    /// <summary>Looks up for stored item by id.</summary>
    bool TryGet(out object item, int id);

    // [Obsolete("Replaced by `GetOrAddViaFactoryDelegate`")]
    // Creates, stores, and returns created item
    // object GetOrAdd(int id, CreateScopedValue createValue, int disposalOrder = 0);

    // todo: @v5 @obsolete split the method into the one with disposalOrder and with the one without
    /// Create the value via `FactoryDelegate` passing the `IResolverContext`
    object GetOrAddViaFactoryDelegate(int id, FactoryDelegate createValue, IResolverContext r, int disposalOrder = 0);

    /// Creates, stores, and returns created item
    object TryGetOrAddWithoutClosure(int id,
        IResolverContext resolveContext, Expression expr, bool useFec,
        Func<IResolverContext, Expression, bool, object> createValue, int disposalOrder = 0);

    /// <summary>Tracked item will be disposed with the scope. 
    /// Smaller <paramref name="disposalOrder"/> will be disposed first.</summary>
    object TrackDisposable(object item, int disposalOrder = 0);

    /// <summary>Tracked item will be disposed with the scope.</summary> 
    T TrackDisposableWithoutDisposalOrder<T>(T disposable) where T : IDisposable;

    ///<summary>Sets or adds the service item directly to the scope services</summary>
    void SetOrAdd(int id, object item);

    //[Obsolete("Removing because it is not used")]
    // object GetOrTryAdd(int id, object item, int disposalOrder);

    ///[Obsolete("Removing because it is not used")]
    void SetUsedInstance(Type type, FactoryDelegate factory);

    /// <summary>Sets (replaces) the factory for specified type.</summary>
    void SetUsedInstance(int typeHash, Type type, FactoryDelegate factory);

    /// Looks up for stored item by type.
    bool TryGetUsedInstance(IResolverContext r, Type type, out object instance);

    // todo: @v5 @api @obsolete switch to the overload below
    /// <summary>Clones the scope.</summary>
    IScope Clone();

    /// <summary>The method will clone the scope factories and already created services,
    /// but may or may not drop the disposables thus ensuring that only the new disposables added in clone will be disposed</summary>
    IScope Clone(bool withDisposables);
  }

  /// <summary>
  /// Scope is container to hold the shared per scope items and dispose <see cref="IDisposable"/> items.
  /// Scope uses Locking to ensure that the object factory called only once.
  /// </summary>
  public sealed class Scope : IScope
  {
    /// <summary>Parent scope in scope stack. Null for the root scope.</summary>
    public IScope Parent { get; }

    /// <summary>Optional name associated with scope.</summary>
    public object Name { get; }

    /// <summary>True if scope is disposed.</summary>
    public bool IsDisposed => _disposed == 1;
    private int _disposed;

    private ImHashMap<Type, FactoryDelegate> _factories;
    private ImList<IDisposable> _unorderedDisposables;
    private ImMap<IDisposable> _disposables;

    internal const int MAP_COUNT = 16;
    internal const int MAP_COUNT_SUFFIX_MASK = MAP_COUNT - 1;
    internal ImMap<object>[] _maps;

    internal static readonly object NoItem = new object();

    // todo: @perf the opportumity to keep it null with the check if it is null, e.g. _maps[index]?.GetValueOrDefault()... will be faster
    private static ImMap<object>[] _emptySlots = CreateEmptyMaps();

    private static ImMap<object>[] CreateEmptyMaps()
    {
      var empty = ImMap<object>.Empty;
      var slots = new ImMap<object>[MAP_COUNT];
      for (var i = 0; i < MAP_COUNT; ++i)
        slots[i] = empty;
      return slots;
    }

    /// <summary>Creates scope with optional parent and name.</summary>
    public Scope(IScope parent = null, object name = null)
        : this(parent, name, CreateEmptyMaps(), ImHashMap<Type, FactoryDelegate>.Empty,
            ImList<IDisposable>.Empty, ImMap<IDisposable>.Empty)
    { }

    private Scope(IScope parent, object name, ImMap<object>[] maps, ImHashMap<Type, FactoryDelegate> factories,
        ImList<IDisposable> unorderedDisposables, ImMap<IDisposable> disposables)
    {
      Parent = parent;
      Name = name;
      _unorderedDisposables = unorderedDisposables;
      _disposables = disposables;
      _factories = factories;
      _maps = maps;
    }

    /// <inheritdoc />
    public IScope Clone() => Clone(true);

    /// <inheritdoc />
    public IScope Clone(bool withDisposables)
    {
      var slotsCopy = new ImMap<object>[MAP_COUNT];
      for (var i = 0; i < MAP_COUNT; i++)
        slotsCopy[i] = _maps[i];

      if (!withDisposables)
        return new Scope(Parent?.Clone(withDisposables), Name, slotsCopy, _factories,
            ImList<IDisposable>.Empty, ImMap<IDisposable>.Empty); // dropping the disposables

      return new Scope(Parent?.Clone(withDisposables), // Не забыть скопировать папу (коментарий для дочки)
          Name, slotsCopy, _factories, _unorderedDisposables, _disposables);
    }

    /// <inheritdoc />
    [Obsolete("Replaced by `GetOrAddViaFactoryDelegate`")]
    public object GetOrAdd(int id, CreateScopedValue createValue, int disposalOrder = 0)
    {
      ref var map = ref _maps[id & MAP_COUNT_SUFFIX_MASK];
      var itemRef = map.GetEntryOrDefault(id);
      if (itemRef != null && itemRef.Value != NoItem)
        return itemRef.Value;
      return TryGetOrAdd(ref map, id, createValue, disposalOrder);
    }

    [Obsolete("Not used - to be removed")]
    private object TryGetOrAdd(ref ImMap<object> map, int id, CreateScopedValue createValue, int disposalOrder = 0)
    {
      if (_disposed == 1)
        Throw.It(Error.ScopeIsDisposed, ToString());

      var itemRef = new ImMapEntry<object>(id, Scope.NoItem);
      var oldMap = map;
      var newMap = oldMap.AddOrKeepEntry(itemRef);
      if (Interlocked.CompareExchange(ref map, newMap, oldMap) != oldMap)
      {
        newMap = Ref.SwapAndGetNewValue(ref map, itemRef, (x, i) => x.AddOrKeepEntry(i));
        var otherItemRef = newMap.GetSurePresentEntry(id);
        if (otherItemRef != itemRef)
          return otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
      }
      else if (newMap == oldMap)
      {
        var otherItemRef = newMap.GetSurePresentEntry(id);
        return otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
      }

      object result = null;
#if SUPPORTS_SPIN_WAIT
      itemRef.Value = result = createValue();
#else
            lock (itemRef) 
            {
                // no need for the double check because this thread is the only one who can create the value
                itemRef.Value = result = createValue();
                Monitor.PulseAll(itemRef);
            }
#endif

      if (result is IDisposable disp && !ReferenceEquals(disp, this))
      {
        if (disposalOrder == 0)
          AddUnorderedDisposable(disp);
        else
          AddDisposable(disp, disposalOrder);
      }

      return result;
    }

    /// <inheritdoc />
    [MethodImpl((MethodImplOptions)256)]
    public object GetOrAddViaFactoryDelegate(int id, FactoryDelegate createValue, IResolverContext r, int disposalOrder = 0)
    {
      var itemRef = _maps[id & MAP_COUNT_SUFFIX_MASK].GetEntryOrDefault(id);
      return itemRef != null
          ? itemRef.Value != NoItem ? itemRef.Value : WaitForItemIsSet(itemRef)
          : TryGetOrAddViaFactoryDelegate(id, createValue, r, disposalOrder);
    }

    internal static readonly MethodInfo GetOrAddViaFactoryDelegateMethod =
        typeof(IScope).GetTypeInfo().GetDeclaredMethod(nameof(IScope.GetOrAddViaFactoryDelegate));

    internal object TryGetOrAddViaFactoryDelegate(int id, FactoryDelegate createValue, IResolverContext r, int disposalOrder = 0)
    {
      if (_disposed == 1)
        Throw.It(Error.ScopeIsDisposed, ToString());

      var itemRef = new ImMapEntry<object>(id, NoItem);
      ref var map = ref _maps[id & MAP_COUNT_SUFFIX_MASK];
      var oldMap = map;
      var newMap = oldMap.AddOrKeepEntry(itemRef);
      if (Interlocked.CompareExchange(ref map, newMap, oldMap) != oldMap)
      {
        newMap = Ref.SwapAndGetNewValue(ref map, itemRef, (x, i) => x.AddOrKeepEntry(i));
        var otherItemRef = newMap.GetSurePresentEntry(id);
        if (otherItemRef != itemRef)
          return otherItemRef.Value != NoItem ? otherItemRef.Value : WaitForItemIsSet(otherItemRef);
      }
      else if (newMap == oldMap)
      {
        var otherItemRef = newMap.GetSurePresentEntry(id);
        return otherItemRef.Value != NoItem ? otherItemRef.Value : WaitForItemIsSet(otherItemRef);
      }

      // todo: @api @perf designing the better ImMap API returning the present item without GetSurePresentEntry call
      // var itemRef = new ImMapEntry<object>(id, Scope.NoItem);
      // var oldMap = map;
      // var oldRefOrNewMap = oldMap.GetOrAddEntry(id, itemRef);
      // if (oldRefOrNewMap is ImMapEntry<object> oldRef && oldMap != ImMap<object>.Empty)
      //     return oldRef.Value != Scope.NoItem ? oldRef.Value : Scope.WaitForItemIsSet(oldRef);

      // if (Interlocked.CompareExchange(ref map, oldRefOrNewMap, oldMap) != oldMap)
      // {
      //     oldRefOrNewMap = Ref.SwapAndGetNewValue(ref map, itemRef, (x, i) => x.AddOrKeepEntry(i));
      //     var otherItemRef = oldRefOrNewMap.GetSurePresentEntry(id);
      //     if (otherItemRef != itemRef)
      //         return otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
      // }

      object result = null;
#if SUPPORTS_SPIN_WAIT
      itemRef.Value = result = createValue(r);
#else
            lock (itemRef) 
            {
                // no need for the double check because this thread is the only one who can create the value
                itemRef.Value = result = createValue(r);
                Monitor.PulseAll(itemRef);
            }
#endif

      if (result is IDisposable disp && !ReferenceEquals(disp, this))
      {
        if (disposalOrder == 0)
          AddUnorderedDisposable(disp);
        else
          AddDisposable(disp, disposalOrder);
      }

      return result;
    }

    /// <summary>The amount of time to wait for the other party to create the scoped (or singleton) service. 
    /// The default value of 5000 ticks rougly corresponds to the 5 seconds.</summary>
    public static uint WaitForScopedServiceIsCreatedTimeoutTicks = 5000;

    internal static object WaitForItemIsSet(ImMapEntry<object> itemRef)
    {
      var tickCount = (uint)Environment.TickCount;
      var tickStart = tickCount;
#if SUPPORTS_SPIN_WAIT
      Debug.WriteLine("SpinWaiting!!! ");

      var spinWait = new SpinWait();
      while (itemRef.Value == NoItem)
      {
        spinWait.SpinOnce();
        if (tickCount - tickStart > WaitForScopedServiceIsCreatedTimeoutTicks)
          Throw.WithDetails(itemRef.Key, Error.WaitForScopedServiceIsCreatedTimeoutExpired, WaitForScopedServiceIsCreatedTimeoutTicks);
        tickCount = (uint)Environment.TickCount;
      }

      Debug.WriteLine("SpinWaiting!!! Done");
#else
            Debug.WriteLine("LockWaiting!!! ");

            lock (itemRef) 
                while (itemRef.Value == NoItem)
                {
                    Monitor.Wait(itemRef);
                    if (tickCount - tickStart > WaitForScopedServiceIsCreatedTimeoutTicks)
                        Throw.WithDetails(itemRef.Key, Error.WaitForScopedServiceIsCreatedTimeoutExpired, WaitForScopedServiceIsCreatedTimeoutTicks);
                    tickCount = (uint)Environment.TickCount;
                }

            Debug.WriteLine("Lock waiting!!! Done");
#endif
      return itemRef.Value;
    }

    [Obsolete("Not used - to be removed")]
    internal ImMapEntry<object> TryAddViaFactoryDelegate(int id, FactoryDelegate createValue, IResolverContext r, int disposalOrder)
    {
      if (_disposed == 1)
        Throw.It(Error.ScopeIsDisposed, ToString());

      var itemRef = new ImMapEntry<object>(id, Scope.NoItem);
      ref var map = ref _maps[id & MAP_COUNT_SUFFIX_MASK];
      var oldMap = map;
      var newMap = oldMap.AddOrKeepEntry(itemRef);
      if (Interlocked.CompareExchange(ref map, newMap, oldMap) != oldMap)
      {
        newMap = Ref.SwapAndGetNewValue(ref map, itemRef, (x, i) => x.AddOrKeepEntry(i));
        var otherItemRef = newMap.GetSurePresentEntry(id);
        if (otherItemRef != itemRef)
        {
          if (otherItemRef.Value == Scope.NoItem)
            Scope.WaitForItemIsSet(otherItemRef);
          return otherItemRef;
        }
      }
      else if (newMap == oldMap)
      {
        var otherItemRef = newMap.GetSurePresentEntry(id);
        if (otherItemRef.Value == Scope.NoItem)
          Scope.WaitForItemIsSet(otherItemRef);
        return otherItemRef;
      }

      object result = null;
#if SUPPORTS_SPIN_WAIT
      itemRef.Value = result = createValue(r);
#else
            lock (itemRef) 
            {
                // no need for the double check because this thread is the only one who can create the value
                itemRef.Value = result = createValue(r);
                Monitor.PulseAll(itemRef);
            }
#endif

      if (result is IDisposable disp && !ReferenceEquals(disp, this))
      {
        if (disposalOrder == 0)
          AddUnorderedDisposable(disp);
        else
          AddDisposable(disp, disposalOrder);
      }

      return itemRef;
    }

    /// <inheritdoc />
    public object TryGetOrAddWithoutClosure(int id,
        IResolverContext resolveContext, Expression expr, bool useFec,
        Func<IResolverContext, Expression, bool, object> createValue, int disposalOrder = 0)
    {
      if (_disposed == 1)
        Throw.It(Error.ScopeIsDisposed, ToString());

      var itemRef = new ImMapEntry<object>(id, NoItem);
      ref var map = ref _maps[id & MAP_COUNT_SUFFIX_MASK];
      var oldMap = map;
      var newMap = oldMap.AddOrKeepEntry(itemRef);
      if (Interlocked.CompareExchange(ref map, newMap, oldMap) != oldMap)
      {
        newMap = Ref.SwapAndGetNewValue(ref map, itemRef, (x, i) => x.AddOrKeepEntry(i));
        var otherItemRef = newMap.GetSurePresentEntry(id);
        if (otherItemRef != itemRef)
          return otherItemRef.Value != NoItem ? otherItemRef.Value : WaitForItemIsSet(otherItemRef);
      }
      else if (newMap == oldMap)
      {
        var otherItemRef = newMap.GetSurePresentEntry(id);
        return otherItemRef.Value != NoItem ? otherItemRef.Value : WaitForItemIsSet(otherItemRef);
      }

      object result = null;
#if SUPPORTS_SPIN_WAIT
      itemRef.Value = result = createValue(resolveContext, expr, useFec);
#else
            lock (itemRef) 
            {
                // no need for the double check because this thread is the only one who can create the value
                itemRef.Value = result = createValue(resolveContext, expr, useFec);
                Monitor.PulseAll(itemRef);
            }
#endif

      if (result is IDisposable disp && !ReferenceEquals(disp, this))
        if (disposalOrder == 0)
          AddUnorderedDisposable(disp);
        else
          AddDisposable(disp, disposalOrder);

      return result;
    }

    ///<inheritdoc />
    public void SetOrAdd(int id, object item)
    {
      if (_disposed == 1)
        Throw.It(Error.ScopeIsDisposed, ToString());

      var itemRef = new ImMapEntry<object>(id, item);
      ref var map = ref _maps[id & MAP_COUNT_SUFFIX_MASK];
      var oldMap = map;
      var newMap = oldMap.AddOrUpdateEntry(itemRef);
      if (Interlocked.CompareExchange(ref map, newMap, oldMap) != oldMap)
        Ref.Swap(ref map, itemRef, (x, i) => x.AddOrUpdateEntry(i));

      if (item is IDisposable disp && !ReferenceEquals(disp, this))
        AddUnorderedDisposable(disp);
    }

    /// [Obsolete("Removing because it is not used")]
    [Obsolete("Removing because it is not used")]
    public object GetOrTryAdd(int id, object newItem, int disposalOrder)
    {
      if (_disposed == 1)
        Throw.It(Error.ScopeIsDisposed, ToString());

      ref var map = ref _maps[id & MAP_COUNT_SUFFIX_MASK];

      var itemRef = new ImMapEntry<object>(id, Scope.NoItem);
      var oldMap = map;
      var newMap = oldMap.AddOrKeepEntry(itemRef);
      if (Interlocked.CompareExchange(ref map, newMap, oldMap) != oldMap)
      {
        newMap = Ref.SwapAndGetNewValue(ref map, itemRef, (x, i) => x.AddOrKeepEntry(i));
        var otherItemRef = newMap.GetSurePresentEntry(id);
        if (otherItemRef != itemRef)
          return otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
      }
      else if (newMap == oldMap)
      {
        var otherItemRef = newMap.GetSurePresentEntry(id);
        return otherItemRef.Value != Scope.NoItem ? otherItemRef.Value : Scope.WaitForItemIsSet(otherItemRef);
      }

#if SUPPORTS_SPIN_WAIT
      itemRef.Value = newItem;
#else
            lock (itemRef) 
            {
                // no need for the double check because this thread is the only one who can create the value
                itemRef.Value = newItem;
                Monitor.PulseAll(itemRef);
            }
#endif

      if (newItem is IDisposable disp && !ReferenceEquals(disp, this))
      {
        if (disposalOrder == 0)
          AddUnorderedDisposable(disp);
        else
          AddDisposable(disp, disposalOrder);
      }

      return newItem;
    }
    internal void AddDisposable(IDisposable disposable, int disposalOrder)
    {
      var d = _disposables;
      if (Interlocked.CompareExchange(ref _disposables, d.AddOrUpdate(disposalOrder, disposable), d) != d)
        Ref.Swap(ref _disposables, disposalOrder, disposable, (x, dispOrder, disp) => x.AddOrUpdate(dispOrder, disp));
    }

    [MethodImpl((MethodImplOptions)256)]
    internal void AddUnorderedDisposable(IDisposable disposable)
    {
      var copy = _unorderedDisposables;
      if (Interlocked.CompareExchange(ref _unorderedDisposables, copy.Push(disposable), copy) != copy)
        Ref.Swap(ref _unorderedDisposables, disposable, (x, d) => x.Push(d));
    }

    /// <inheritdoc />
    [MethodImpl((MethodImplOptions)256)]
    public bool TryGet(out object item, int id)
    {
      var itemRef = _maps[id & MAP_COUNT_SUFFIX_MASK].GetEntryOrDefault(id);
      if (itemRef != null && itemRef.Value != NoItem)
      {
        item = itemRef.Value;
        return true;
      }

      item = null;
      return false;
    }

    // todo: @perf consider adding the overload without `disposalOrder`
    // todo: @perf we always know that item is IDisposable because it is being checked upper in stack, so we may remove the check here 
    /// <summary>Can be used to manually add service for disposal</summary>
    public object TrackDisposable(object item, int disposalOrder = 0)
    {
      if (item is IDisposable disp && !ReferenceEquals(disp, this))
        if (disposalOrder == 0)
          AddUnorderedDisposable(disp);
        else
          AddDisposable(disp, disposalOrder);
      return item;
    }

    internal static readonly MethodInfo TrackDisposableMethod =
        typeof(IScope).GetTypeInfo().GetDeclaredMethod(nameof(IScope.TrackDisposable));

    /// <summary>Tracked item will be disposed with the scope.</summary> 
    public T TrackDisposableWithoutDisposalOrder<T>(T disposable) where T : IDisposable
    {
      if (!ReferenceEquals(disposable, this))
      {
        var copy = _unorderedDisposables;
        if (Interlocked.CompareExchange(ref _unorderedDisposables, copy.Push(disposable), copy) != copy)
          Ref.Swap(ref _unorderedDisposables, disposable, (x, d) => x.Push(d));
      }
      return disposable;
    }

    ///[Obsolete("Removing because it is not used")]
    public void SetUsedInstance(Type type, FactoryDelegate factory) =>
        SetUsedInstance(RuntimeHelpers.GetHashCode(type), type, factory);

    /// <inheritdoc />
    public void SetUsedInstance(int typeHash, Type type, FactoryDelegate factory)
    {
      if (_disposed == 1)
        Throw.It(Error.ScopeIsDisposed, ToString());
      var f = _factories;
      if (Interlocked.CompareExchange(ref _factories, f.AddOrUpdate(typeHash, type, factory), f) != f)
        Ref.Swap(ref _factories, typeHash, type, factory, (x, h, t, fac) => x.AddOrUpdate(h, t, fac));
    }

    /// <summary>Try retrieve instance from the small registry.</summary>
    public bool TryGetUsedInstance(IResolverContext r, Type type, out object instance)
    {
      instance = null;
      if (_disposed == 1)
        return false;

      if (!_factories.IsEmpty)
      {
        var factory = _factories.GetValueOrDefault(RuntimeHelpers.GetHashCode(type), type);
        if (factory != null)
        {
          instance = factory(r);
          return true;
        }
      }

      return Parent?.TryGetUsedInstance(r, type, out instance) ?? false;
    }

    /// <summary>Enumerates all the parent scopes upwards starting from this one.</summary>
    public IEnumerator<IScope> GetEnumerator()
    {
      for (IScope scope = this; scope != null; scope = scope.Parent)
        yield return scope;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Disposes all stored <see cref="IDisposable"/> objects and empties item storage.
    /// The disposal happens in REVERSE resolution / injection order, consumer first, dependency next.
    /// It will allow consumer to do something with its dependency before it is disposed.</summary>
    /// <remarks>All disposal exceptions are swallowed except the ContainerException,
    /// which may indicate container misconfiguration.</remarks>
    public void Dispose()
    {
      if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
        return;

      if (!_disposables.IsEmpty)
        SafelyDisposeOrderedDisposables(_disposables);

      for (var unordDisp = _unorderedDisposables; !unordDisp.IsEmpty; unordDisp = unordDisp.Tail)
        unordDisp.Head.Dispose();

      _unorderedDisposables = ImList<IDisposable>.Empty;
      _disposables = ImMap<IDisposable>.Empty;
      _factories = ImHashMap<Type, FactoryDelegate>.Empty;

      var maps = Interlocked.Exchange(ref _maps, _emptySlots);
      var empty = ImMap<object>.Empty;
      //for (int i = 0; i < MAP_COUNT; i++) maps[i] = empty;
      //_mapsPool.Return(maps);
    }

    private static void SafelyDisposeOrderedDisposables(ImMap<IDisposable> disposables)
    {
      disposables.Visit(d => {
        try
        {
          // Ignoring disposing exception, as it is not important to proceed the disposal of other items
          d.Value.Dispose();
        }
        catch (ContainerException)
        {
          throw;
        }
        catch (Exception)
        {
        }
      });
    }

    /// <summary>Prints scope info (name and parent) to string for debug purposes.</summary>
    public override string ToString() =>
        "{" + (IsDisposed ? "IsDisposed=true, " : "")
            + (Name != null ? "Name=" + Name : "Name=null")
            + (Parent != null ? ", Parent=" + Parent : "")
            + "}";
  }

  /// <summary>Delegate to get new scope from old/existing current scope.</summary>
  /// <param name="oldScope">Old/existing scope to change.</param>
  /// <returns>New scope or old if do not want to change current scope.</returns>
  public delegate IScope SetCurrentScopeHandler(IScope oldScope);

  /// <summary>Provides ambient current scope and optionally scope storage for container,
  /// examples are HttpContext storage, Execution context, Thread local.</summary>
  public interface IScopeContext : IDisposable
  {
    /// <summary>Returns current scope or null if no ambient scope available at the moment.</summary>
    /// <returns>Current scope or null.</returns>
    IScope GetCurrentOrDefault();

    /// <summary>Changes current scope using provided delegate. Delegate receives current scope as input and
    /// should return new current scope.</summary>
    /// <param name="setCurrentScope">Delegate to change the scope.</param>
    /// <remarks>Important: <paramref name="setCurrentScope"/> may be called multiple times in concurrent environment.
    /// Make it predictable by removing any side effects.</remarks>
    /// <returns>New current scope. So it is convenient to use method in "using (var newScope = ctx.SetCurrent(...))".</returns>
    IScope SetCurrent(SetCurrentScopeHandler setCurrentScope);
  }

#if NET35 || NET40 || NET403 || PCL || PCL328 || PCL259

    /// <summary>Tracks one current scope per thread, so the current scope in different tread would be different or null,
    /// if not yet tracked. Context actually stores scope references internally, so it should be disposed to free them.</summary>
    public sealed class ThreadScopeContext : IScopeContext
    {
        /// <summary>Provides static name for context. It is OK because its constant.</summary>
        public static readonly string ScopeContextName = "ThreadScopeContext";

        /// <summary>Returns current scope in calling Thread or null, if no scope tracked.</summary>
        public IScope GetCurrentOrDefault() =>
            _scopes.GetValueOrDefault(Portable.GetCurrentManagedThreadID()) as IScope;

        /// <summary>Change current scope for the calling Thread.</summary>
        public IScope SetCurrent(SetCurrentScopeHandler setCurrentScope)
        {
            var threadId = Portable.GetCurrentManagedThreadID();
            IScope newScope = null;
            Ref.Swap(ref _scopes, s => s.AddOrUpdate(threadId, 
                newScope = setCurrentScope(s.GetValueOrDefault(threadId) as IScope)));
            return newScope;
        }

        /// <summary>Disposes the scopes and empties internal scope storage.</summary>
        public void Dispose()
        {
            if (!_scopes.IsEmpty) 
                _scopes.Visit(d => d.Value?.Dispose());
            _scopes = ImMap<IScope>.Empty;
        }

        /// Collection of scoped by their managed thread id
        private ImMap<IScope> _scopes = ImMap<IScope>.Empty;
    }

#else
  /// <summary>Tracks one current scope per thread, so the current scope in different tread would be different or null,
  /// if not yet tracked. Context actually stores scope references internally, so it should be disposed to free them.</summary>
  public sealed class ThreadScopeContext : IScopeContext
  {
    /// <summary>Provides static name for context. It is OK because its constant.</summary>
    public static readonly string ScopeContextName = "ThreadScopeContext";

    private ThreadLocal<IScope> _scope = new ThreadLocal<IScope>(true);

    /// <summary>Returns current scope in calling Thread or null, if no scope tracked.</summary>
    public IScope GetCurrentOrDefault() =>
        _scope.Value;

    /// <summary>Change current scope for the calling Thread.</summary>
    public IScope SetCurrent(SetCurrentScopeHandler setCurrentScope) =>
        _scope.Value = setCurrentScope(GetCurrentOrDefault());

    /// <summary>Disposes the scopes and empties internal scope storage.</summary>
    public void Dispose()
    {
      var scopes = _scope.Values;
      foreach (var scope in scopes)
      {
        var s = scope;
        while (s != null)
        {
          var x = s;
          s = s.Parent;
          x.Dispose();
        }
      }
    }
  }

#endif

  /// <summary>Simplified scope agnostic reuse abstraction. More easy to implement,
  ///  and more powerful as can be based on other storage beside reuse.</summary>
  public interface IReuse : IConvertibleToExpression
  {
    /// <summary>Relative to other reuses lifespan value.</summary>
    int Lifespan { get; }

    /// <summary>Optional name. Use to find matching scope by the name.
    /// It also may be interpreted as object[] Names for matching with multiple scopes </summary>
    object Name { get; }

    /// <summary>Returns true if reuse can be applied: may check if scope or other reused item storage is present.</summary>
    bool CanApply(Request request);

    /// <summary>Returns composed expression.</summary>
    Expression Apply(Request request, Expression serviceFactoryExpr);
  }

  /// <summary>Returns container bound scope for storing singleton objects.</summary>
  public sealed class SingletonReuse : IReuse
  {
    /// <summary>Big lifespan.</summary>
    public const int DefaultLifespan = 1000;

    /// <summary>Relative to other reuses lifespan value.</summary>
    public int Lifespan => DefaultLifespan;

    /// <inheritdoc />
    public object Name => null;

    /// <summary>Returns true because singleton is always available.</summary>
    public bool CanApply(Request request) => true;

    /// <summary>Returns expression call to GetOrAddItem.</summary>
    public Expression Apply(Request request, Expression serviceFactoryExpr)
    {
      // this is required because we cannot use ValueType for the object
      if (serviceFactoryExpr.Type.IsValueType())
        serviceFactoryExpr = Convert(serviceFactoryExpr, typeof(object));

      if (request.TracksTransientDisposable)
        return Call(ResolverContext.SingletonScopeExpr, Scope.TrackDisposableMethod,
            serviceFactoryExpr, Constant(request.Factory.Setup.DisposalOrder));

      var factoryId = request.FactoryType == FactoryType.Decorator
          ? request.CombineDecoratorWithDecoratedFactoryID() : request.FactoryID;


      var lambdaExpr = Lambda<FactoryDelegate>(serviceFactoryExpr,
          FactoryDelegateCompiler.FactoryDelegateParamExprs
#if SUPPORTS_FAST_EXPRESSION_COMPILER
                , typeof(object)
#endif
            );

      if (request.DependencyCount > 0)
        request.DecreaseTrackedDependencyCountForParents(request.DependencyCount);

      return Call(ResolverContext.SingletonScopeExpr, Scope.GetOrAddViaFactoryDelegateMethod,
          Constant(factoryId), lambdaExpr, FactoryDelegateCompiler.ResolverContextParamExpr, Constant(request.Factory.Setup.DisposalOrder));
    }

    private static readonly Lazy<Expression> _singletonReuseExpr = Lazy.Of<Expression>(() =>
        Field(null, typeof(Reuse).Field(nameof(Reuse.Singleton))));

    /// <inheritdoc />
    public Expression ToExpression(Func<object, Expression> fallbackConverter) => _singletonReuseExpr.Value;

    /// <summary>Pretty prints reuse name and lifespan</summary>
    public override string ToString() => "Singleton {Lifespan=" + Lifespan + "}";
  }

  /// <summary>Specifies that instances are created, stored and disposed together with some scope.</summary>
  public sealed class CurrentScopeReuse : IReuse
  {
    /// <summary>Less than Singleton's</summary>
    public const int DefaultLifespan = 100;

    /// <summary>Relative to other reuses lifespan value.</summary>
    public int Lifespan { get; }

    /// <inheritdoc />
    public object Name { get; }

    /// <summary>Returns true if scope is open and the name is matching with reuse <see cref="Name"/>.</summary>
    public bool CanApply(Request request) =>
        ScopedOrSingleton ||
        (Name == null ? request.Container.CurrentScope != null : request.Container.GetNamedScope(Name, false) != null);

    /// <summary>Creates scoped item creation and access expression.</summary>
    public Expression Apply(Request request, Expression serviceFactoryExpr)
    {
      // strip the conversion as we are operating with object anyway
      if (serviceFactoryExpr.NodeType == ExprType.Convert)
        serviceFactoryExpr = ((UnaryExpression)serviceFactoryExpr).Operand;

      // this is required because we cannot use ValueType for the object
      if (serviceFactoryExpr.Type.IsValueType())
        serviceFactoryExpr = Convert(serviceFactoryExpr, typeof(object));

      var resolverContextParamExpr = FactoryDelegateCompiler.ResolverContextParamExpr;
      if (request.TracksTransientDisposable)
      {
        if (ScopedOrSingleton)
          return Call(TrackScopedOrSingletonMethod, resolverContextParamExpr, serviceFactoryExpr);

        var ifNoScopeThrowExpr = Constant(request.IfUnresolved == IfUnresolved.Throw);
        if (Name == null)
          return Call(TrackScopedMethod, resolverContextParamExpr, ifNoScopeThrowExpr, serviceFactoryExpr);

        var nameExpr = request.Container.GetConstantExpression(Name, typeof(object));
        return Call(TrackNameScopedMethod, resolverContextParamExpr, nameExpr, ifNoScopeThrowExpr, serviceFactoryExpr);
      }
      else
      {
        var idExpr = Constant(request.FactoryType == FactoryType.Decorator ?
            request.CombineDecoratorWithDecoratedFactoryID() : request.FactoryID);

        Expression factoryDelegateExpr;
        if (serviceFactoryExpr is InvocationExpression ie &&
            ie.Expression is ConstantExpression registeredDelegateExpr &&
            registeredDelegateExpr.Type == typeof(FactoryDelegate))
        {
          factoryDelegateExpr = registeredDelegateExpr;
        }
        else
        {
          factoryDelegateExpr = Lambda<FactoryDelegate>(serviceFactoryExpr,
              FactoryDelegateCompiler.FactoryDelegateParamExprs
#if SUPPORTS_FAST_EXPRESSION_COMPILER
                        , typeof(object)
#endif
                    );

          // decrease the dependency count when wrapping into lambda
          if (request.DependencyCount > 0)
            request.DecreaseTrackedDependencyCountForParents(request.DependencyCount);
        }

        var disposalIndex = request.Factory.Setup.DisposalOrder;

        if (ScopedOrSingleton)
          return Call(GetScopedOrSingletonViaFactoryDelegateMethod,
              resolverContextParamExpr, idExpr, factoryDelegateExpr, Constant(disposalIndex));

        var ifNoScopeThrowExpr = Constant(request.IfUnresolved == IfUnresolved.Throw);

        if (Name == null)
        {
          if (disposalIndex == 0)
            return Call(GetScopedViaFactoryDelegateNoDisposalIndexMethod,
                resolverContextParamExpr, ifNoScopeThrowExpr, idExpr, factoryDelegateExpr);

          return Call(GetScopedViaFactoryDelegateMethod,
              resolverContextParamExpr, ifNoScopeThrowExpr, idExpr, factoryDelegateExpr, Constant(disposalIndex));
        }

        return Call(GetNameScopedViaFactoryDelegateMethod, resolverContextParamExpr,
            request.Container.GetConstantExpression(Name, typeof(object)),
            ifNoScopeThrowExpr, idExpr, factoryDelegateExpr, Constant(disposalIndex));
      }
    }

    /// <inheritdoc />
    public Expression ToExpression(Func<object, Expression> fallbackConverter) =>
        Name == null && !ScopedOrSingleton
            ? Field(null, typeof(Reuse).GetTypeInfo().GetDeclaredField(nameof(Reuse.Scoped)))
            : ScopedOrSingleton
                ? (Expression)Field(null, typeof(Reuse).GetTypeInfo().GetDeclaredField(nameof(Reuse.ScopedOrSingleton)))
                : Call(typeof(Reuse).Method("ScopedTo", typeof(object)), fallbackConverter(Name));

    /// <summary>Pretty prints reuse to string.</summary> <returns>Reuse string.</returns>
    public override string ToString()
    {
      var s = new StringBuilder(ScopedOrSingleton ? "ScopedOrSingleton {" : "Scoped {");
      if (Name != null)
        s.Append("Name=").Print(Name).Append(", ");
      if (Lifespan != DefaultLifespan)
        s.Append("NON DEFAULT LIFESPAN=").Append(Lifespan);
      else
        s.Append("Lifespan=").Append(Lifespan);

      return s.Append("}").ToString();
    }

    /// <summary>Creates the reuse.</summary>
    public CurrentScopeReuse(object name, bool scopedOrSingleton, int lifespan)
    {
      Name = name;
      ScopedOrSingleton = scopedOrSingleton;
      Lifespan = lifespan;
    }

    /// <summary>Creates the reuse optionally specifying its name.</summary>
    public CurrentScopeReuse(object name = null, bool scopedOrSingleton = false)
        : this(name, scopedOrSingleton, DefaultLifespan)
    {
    }

    /// <summary>Flag indicating that it is a scope or singleton.</summary>
    public readonly bool ScopedOrSingleton;

    // [Obsolete("Replaced by `GetScopedOrSingletonViaFactoryDelegate`")]
    // public static object GetScopedOrSingleton(IResolverContext r,
    //     int id, CreateScopedValue createValue, int disposalIndex) =>
    //     (r.CurrentScope ?? r.SingletonScope).GetOrAdd(id, createValue, disposalIndex);

    /// Subject
    public static object GetScopedOrSingletonViaFactoryDelegate(IResolverContext r,
        int id, FactoryDelegate createValue, int disposalIndex) =>
        (r.CurrentScope ?? r.SingletonScope).GetOrAddViaFactoryDelegate(id, createValue, r, disposalIndex);

    internal static readonly MethodInfo GetScopedOrSingletonViaFactoryDelegateMethod =
        typeof(CurrentScopeReuse).GetTypeInfo().GetDeclaredMethod(nameof(GetScopedOrSingletonViaFactoryDelegate));

    /// <summary>Tracks the Unordered disposal in the current scope or in the singleton as fallback</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static object TrackScopedOrSingleton(IResolverContext r, object item) =>
        item is IDisposable d ? (r.CurrentScope ?? r.SingletonScope).TrackDisposableWithoutDisposalOrder(d) : item;

    internal static readonly MethodInfo TrackScopedOrSingletonMethod =
        typeof(CurrentScopeReuse).GetTypeInfo().GetDeclaredMethod(nameof(TrackScopedOrSingleton));

    // [Obsolete("Replaced by `GetScopedViaFactoryDelegate`")]
    // public static object GetScoped(IResolverContext r,
    //     bool throwIfNoScope, int id, CreateScopedValue createValue, int disposalIndex) =>
    //     r.GetCurrentScope(throwIfNoScope)?.GetOrAdd(id, createValue, disposalIndex);

    /// Subject
    public static object GetScopedViaFactoryDelegateNoDisposalIndex(IResolverContext r,
        bool throwIfNoScope, int id, FactoryDelegate createValue) =>
        r.GetCurrentScope(throwIfNoScope)?.GetOrAddViaFactoryDelegate(id, createValue, r);

    internal static readonly MethodInfo GetScopedViaFactoryDelegateNoDisposalIndexMethod =
        typeof(CurrentScopeReuse).GetTypeInfo().GetDeclaredMethod(nameof(GetScopedViaFactoryDelegateNoDisposalIndex));

    /// Subject
    public static object GetScopedViaFactoryDelegate(IResolverContext r,
        bool throwIfNoScope, int id, FactoryDelegate createValue, int disposalIndex) =>
        r.GetCurrentScope(throwIfNoScope)?.GetOrAddViaFactoryDelegate(id, createValue, r, disposalIndex);

    internal static readonly MethodInfo GetScopedViaFactoryDelegateMethod =
        typeof(CurrentScopeReuse).GetTypeInfo().GetDeclaredMethod(nameof(GetScopedViaFactoryDelegate));

    // [Obsolete("Replaced by `GetNameScopedViaFactoryDelegate`")]
    // public static object GetNameScoped(IResolverContext r,
    //     object scopeName, bool throwIfNoScope, int id, CreateScopedValue createValue, int disposalIndex) =>
    //     r.GetNamedScope(scopeName, throwIfNoScope)?.GetOrAdd(id, createValue, disposalIndex);

    /// Subject
    public static object GetNameScopedViaFactoryDelegate(IResolverContext r,
        object scopeName, bool throwIfNoScope, int id, FactoryDelegate createValue, int disposalIndex) =>
        r.GetNamedScope(scopeName, throwIfNoScope)?.GetOrAddViaFactoryDelegate(id, createValue, r, disposalIndex);

    internal static readonly MethodInfo GetNameScopedViaFactoryDelegateMethod =
        typeof(CurrentScopeReuse).GetTypeInfo().GetDeclaredMethod(nameof(GetNameScopedViaFactoryDelegate));

    /// Subject
    public static object TrackScoped(IResolverContext r, bool throwIfNoScope, object item) =>
        item is IDisposable d ? r.GetCurrentScope(throwIfNoScope)?.TrackDisposableWithoutDisposalOrder(d) : item;

    internal static readonly MethodInfo TrackScopedMethod =
        typeof(CurrentScopeReuse).GetTypeInfo().GetDeclaredMethod(nameof(TrackScoped));

    /// Subject
    public static object TrackNameScoped(IResolverContext r, object scopeName, bool throwIfNoScope, object item) =>
        item is IDisposable d ? r.GetNamedScope(scopeName, throwIfNoScope)?.TrackDisposableWithoutDisposalOrder(d) : item;

    internal static readonly MethodInfo TrackNameScopedMethod =
        typeof(CurrentScopeReuse).GetTypeInfo().GetDeclaredMethod(nameof(TrackNameScoped));
  }

  /// <summary>Abstracts way to match reuse and scope names</summary>
  public interface IScopeName
  {
    /// <summary>Does the job.</summary>
    bool Match(object scopeName);
  }

  /// <summary>Represents multiple names</summary>
  public sealed class CompositeScopeName : IScopeName
  {
    /// <summary>Wraps multiple names</summary>
    public static CompositeScopeName Of(object[] names) => new CompositeScopeName(names);

    /// <summary>Matches all the name in a loop until first match is found, otherwise returns false.</summary>
    public bool Match(object scopeName)
    {
      var names = _names;
      for (var i = 0; i < names.Length; i++)
      {
        var name = names[i];
        if (name == scopeName)
          return true;
        var aScopeName = name as IScopeName;
        if (aScopeName != null && aScopeName.Match(scopeName))
          return true;
        if (scopeName != null && scopeName.Equals(name))
          return true;
      }

      return false;
    }

    private CompositeScopeName(object[] names) { _names = names; }
    private readonly object[] _names;
  }

  /// <summary>Holds the name for the resolution scope.</summary>
  public sealed class ResolutionScopeName : IScopeName
  {
    /// <summary>Creates scope with specified service type and key</summary>
    public static ResolutionScopeName Of(Type serviceType = null, object serviceKey = null) =>
        new ResolutionScopeName(serviceType, serviceKey);

    /// <summary>Creates scope with specified service type and key.</summary>
    public static ResolutionScopeName Of<TService>(object serviceKey = null) =>
        new ResolutionScopeName(typeof(TService), serviceKey);

    /// <summary>Type of service opening the scope.</summary>
    public readonly Type ServiceType;

    /// <summary>Optional service key of service opening the scope.</summary>
    public readonly object ServiceKey;

    private ResolutionScopeName(Type serviceType, object serviceKey)
    {
      ServiceType = serviceType;
      ServiceKey = serviceKey;
    }

    /// <inheritdoc />
    public bool Match(object scopeName)
    {
      var name = scopeName as ResolutionScopeName;
      return name != null &&
             (ServiceType == null ||
             name.ServiceType.IsAssignableTo(ServiceType) ||
             ServiceType.IsOpenGeneric() &&
             name.ServiceType.GetGenericDefinitionOrNull().IsAssignableTo(ServiceType)) &&
             (ServiceKey == null || ServiceKey.Equals(name.ServiceKey));
    }

    /// <summary>String representation for easy debugging and understood error messages.</summary>
    public override string ToString()
    {
      var s = new StringBuilder().Append("{ServiceType=").Print(ServiceType);
      if (ServiceKey != null)
        s.Append(", ServiceKey=").Print(ServiceKey);
      return s.Append('}').ToString();
    }
  }

  /// <summary>Specifies pre-defined reuse behaviors supported by container:
  /// used when registering services into container with <see cref="Registrator"/> methods.</summary>
  public static class Reuse
  {
    /// <summary>Synonym for absence of reuse.</summary>
    public static readonly IReuse Transient = new TransientReuse();

    /// <summary>Specifies to store single service instance per <see cref="Container"/>.</summary>
    public static readonly IReuse Singleton = new SingletonReuse();

    /// <summary>Same as InCurrentScope. From now on will be the default name.</summary>
    public static readonly IReuse Scoped = new CurrentScopeReuse();

    /// <summary>Same as InCurrentNamedScope. From now on will be the default name.</summary>
    public static IReuse ScopedTo(object name) => new CurrentScopeReuse(name);

    /// <summary>Specifies all the scope details</summary>
    public static IReuse ScopedTo(object name, bool scopedOrSingleton, int lifespan) =>
        new CurrentScopeReuse(name, scopedOrSingleton, lifespan);

    /// <summary>Scoped to multiple names.</summary>
    public static IReuse ScopedTo(params object[] names) =>
        names.IsNullOrEmpty() ? Scoped
        : names.Length == 1 ? ScopedTo(names[0])
        : new CurrentScopeReuse(CompositeScopeName.Of(names));

    // todo: @api @v5 Consider changing the name (say to ScopedToService) to remove the ambiguity
    /// <summary>[Obsolete("Use ScopedToService to prevent ambiguity with the ScopeTo(object name) where name is the Type")]</summary>
    [Obsolete("Use ScopedToService to prevent ambiguity with the ScopeTo(object name) where name is the Type")]
    public static IReuse ScopedTo(Type serviceType = null, object serviceKey = null) =>
        serviceType == null && serviceKey == null ? Scoped
        : new CurrentScopeReuse(ResolutionScopeName.Of(serviceType, serviceKey));

    /// <summary>Scoped to the scope created by the service with the specified type and optional key</summary>
    public static IReuse ScopedToService(Type serviceType = null, object serviceKey = null) =>
        serviceType == null && serviceKey == null ? Scoped
        : new CurrentScopeReuse(ResolutionScopeName.Of(serviceType, serviceKey));

    /// <summary>Scoped to the scope created by the service with the specified type and optional key</summary>
    public static IReuse ScopedTo<TService>(object serviceKey = null) =>
        ScopedToService(typeof(TService), serviceKey);

    /// <summary>Scoped to the scope created by the service with the specified type and optional key</summary>
    public static IReuse ScopedToService<TService>(object serviceKey = null) =>
        ScopedToService(typeof(TService), serviceKey);

    /// <summary>The same as <see cref="InCurrentScope"/> but if no open scope available will fallback to <see cref="Singleton"/></summary>
    /// <remarks>The <see cref="Error.DependencyHasShorterReuseLifespan"/> is applied the same way as for <see cref="InCurrentScope"/> reuse.</remarks>
    public static readonly IReuse ScopedOrSingleton = new CurrentScopeReuse(scopedOrSingleton: true);

    /// <summary>Obsolete: same as <see cref="Scoped"/>.</summary>
    [Obsolete("The same as Reuse.Scoped, please prefer to use Reuse.Scoped or the Reuse.ScopedTo to specify a bound service.")]
    public static readonly IReuse InResolutionScope = Scoped;

    /// <summary>Obsolete: same as <see cref="Scoped"/>.</summary>
    public static readonly IReuse InCurrentScope = Scoped;

    /// <summary>Returns current scope reuse with specific name to match with scope.
    /// If name is not specified then function returns <see cref="InCurrentScope"/>.</summary>
    /// <param name="name">(optional) Name to match with scope.</param>
    /// <returns>Created current scope reuse.</returns>
    public static IReuse InCurrentNamedScope(object name = null) => ScopedTo(name);

    /// <summary>Obsolete: will be soon - please use ScopedToService instead.</summary>
    public static IReuse InResolutionScopeOf(Type assignableFromServiceType = null, object serviceKey = null) =>
        ScopedToService(assignableFromServiceType, serviceKey);

    /// <summary>Obsolete: will be soon - please use ScopedToService instead.</summary>
    public static IReuse InResolutionScopeOf<TAssignableFromServiceType>(object serviceKey = null) =>
        ScopedToService<TAssignableFromServiceType>(serviceKey);

    /// <summary>Same as Scoped but requires <see cref="ThreadScopeContext"/>.</summary>
    public static readonly IReuse InThread = Scoped;

    // todo: Minimize usage of name for scopes, it will be more performant. e.g. ASP.NET Core does not use one.
    /// <summary>Special name that by convention recognized by <see cref="InWebRequest"/>.</summary>
    public static string WebRequestScopeName = "~WebRequestScopeName";

    /// <summary>Obsolete: please prefer using <see cref="Reuse.Scoped"/> instead.
    /// The named scope has performance drawback comparing to just a scope.
    /// If you need to distinguish nested scope, give names to them instead of naming the top web request scope.</summary>
    public static readonly IReuse InWebRequest = ScopedTo(WebRequestScopeName);

    #region Implementation

    private sealed class TransientReuse : IReuse
    {
      public int Lifespan => 0;

      public object Name => null;

      public Expression Apply(Request _, Expression serviceFactoryExpr) => serviceFactoryExpr;

      public bool CanApply(Request request) => true;

      private readonly Lazy<Expression> _transientReuseExpr = Lazy.Of<Expression>(() =>
          Field(null, typeof(Reuse).Field(nameof(Transient))));

      public Expression ToExpression(Func<object, Expression> fallbackConverter) =>
          _transientReuseExpr.Value;

      public override string ToString() => "TransientReuse";
    }

    #endregion
  }

  /// <summary>Policy to handle unresolved service.</summary>
  public enum IfUnresolved
  {
    /// <summary>If service is unresolved for whatever means, the Resolve will throw the respective exception.</summary>
    Throw,
    /// <summary>If service is unresolved for whatever means, the Resolve will return the default value.</summary>
    ReturnDefault,
    /// <summary>If service is not registered, then the Resolve will return the default value, for the other errors it will throw.</summary>
    ReturnDefaultIfNotRegistered,
  }

  /// <summary>Declares minimal API for service resolution. 
  /// Resolve default and keyed is separated because of optimization for faster resolution of the former.</summary>
  public interface IResolver
#if SUPPORTS_ISERVICE_PROVIDER
    : IServiceProvider
#endif

  {
    /// <summary>Resolves default (non-keyed) service from container and returns created service object.</summary>
    /// <param name="serviceType">Service type to search and to return.</param>
    /// <param name="ifUnresolved">Says what to do if service is unresolved.</param>
    /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> provided.</returns>
    object Resolve(Type serviceType, IfUnresolved ifUnresolved);

    /// <summary>Resolves service instance from container.</summary>
    /// <param name="serviceType">Service type to search and to return.</param>
    /// <param name="serviceKey">(optional) service key used for registering service.</param>
    /// <param name="ifUnresolved">(optional) Says what to do if service is unresolved.</param>
    /// <param name="requiredServiceType">(optional) Registered or wrapped service type to use instead of <paramref name="serviceType"/>,
    ///     or wrapped type for generic wrappers.  The type should be assignable to return <paramref name="serviceType"/>.</param>
    /// <param name="preResolveParent">(optional) Dependency chain info.</param>
    /// <param name="args">(optional) To specify the dependency objects to use instead of resolving them from container.</param>
    /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> parameter.</returns>
    object Resolve(Type serviceType, object serviceKey,
        IfUnresolved ifUnresolved, Type requiredServiceType, Request preResolveParent, object[] args);

    /// <summary>Resolves all services registered for specified <paramref name="serviceType"/>, or if not found returns
    /// empty enumerable. If <paramref name="serviceType"/> specified then returns only (single) service registered with this type.</summary>
    /// <param name="serviceType">Return type of an service item.</param>
    /// <param name="serviceKey">(optional) Resolve only single service registered with the key.</param>
    /// <param name="requiredServiceType">(optional) Actual registered service to search for.</param>
    /// <param name="preResolveParent">Dependency resolution path info.</param>
    /// <param name="args">(optional) To specify the dependency objects to use instead of resolving them from container.</param>
    /// <returns>Enumerable of found services or empty. Does Not throw if no service found.</returns>
    IEnumerable<object> ResolveMany(Type serviceType, object serviceKey,
        Type requiredServiceType, Request preResolveParent, object[] args);
  }

  /// <summary>Specifies options to handle situation when registered service is already present in the registry.</summary>
  public enum IfAlreadyRegistered
  {
    /// <summary>Appends new default registration or throws registration with the same key.</summary>
    AppendNotKeyed,
    /// <summary>Throws if default or registration with the same key is already exist.</summary>
    Throw,
    /// <summary>Keeps old default or keyed registration ignoring new registration: ensures Register-Once semantics.</summary>
    Keep,
    /// <summary>Replaces old registration with new one.</summary>
    Replace,
    /// <summary>Adds the new implementation or null (Made.Of),
    /// otherwise keeps the previous registration of the same implementation type.</summary>
    AppendNewImplementation
  }

  /// <summary>Existing registration info.</summary>
  public struct ServiceRegistrationInfo : IComparable<ServiceRegistrationInfo>
  {
    /// <summary>Registered factory.</summary>
    public Factory Factory;

    /// <summary>Required service type.</summary>
    public Type ServiceType;

    /// <summary>May be <c>null</c> for single default service, or <see cref="DefaultKey"/> for multiple default services.</summary>
    public object OptionalServiceKey;

    /// <summary>Provides registration order across all factory registrations in container.</summary>
    /// <remarks>May be the same for factory registered with multiple services
    /// OR for closed-generic factories produced from the single open-generic registration.</remarks>
    public int FactoryRegistrationOrder => Factory.RegistrationOrder;

    /// <summary>Implementation type if available.</summary>
    public Type ImplementationType => Factory.CanAccessImplementationType ? Factory.ImplementationType : null;

    /// <summary>Shortcut to <see cref="Setup.AsResolutionRoot"/> property, useful to find all roots</summary>
    public bool AsResolutionRoot => Factory.Setup.AsResolutionRoot;

    /// <summary>Shortcut to service info.</summary>
    public ServiceInfo ToServiceInfo() => ServiceInfo.Of(ServiceType, serviceKey: OptionalServiceKey);

    /// <summary>Overrides the service type and pushes the original service type to required service type</summary>
    public ServiceInfo ToServiceInfo(Type serviceType) =>
        ServiceInfo.Of(serviceType, ServiceType, IfUnresolved.Throw, OptionalServiceKey);

    /// <summary>Overrides the service type and pushes the original service type to required service type</summary>
    public ServiceInfo ToServiceInfo<TService>() => ToServiceInfo(typeof(TService));

    /// <summary>Creates info. Registration order is figured out automatically based on Factory.</summary>
    public ServiceRegistrationInfo(Factory factory, Type serviceType, object optionalServiceKey)
    {
      Factory = factory;
      ServiceType = serviceType;
      OptionalServiceKey = optionalServiceKey;
    }

    /// <summary>Orders by registration</summary>
    public int CompareTo(ServiceRegistrationInfo other) => Factory.FactoryID - other.Factory.FactoryID;

    /// <summary>Pretty-prints info to string.</summary>
    public override string ToString()
    {
      var s = new StringBuilder("ServiceType=`").Print(ServiceType).Append('`');
      if (OptionalServiceKey != null)
        s.Append(" with ServiceKey=`").Print(OptionalServiceKey).Append('`');
      return s.Append(" with Factory=`").Append(Factory).Append('`').ToString();
    }
  }

  /// <summary>Defines operations that for changing registry, and checking if something exist in registry.</summary>
  public interface IRegistrator
  {
    /// <summary>Registers factory in registry with specified service type and key for lookup.
    /// Returns true if factory was added to registry, false otherwise. False may be in case of <see cref="IfAlreadyRegistered.Keep"/>
    /// setting and already existing factory</summary>
    /// <param name="factory">To register.</param>
    /// <param name="serviceType">Service type as unique key in registry for lookup.</param>
    /// <param name="serviceKey">Service key as complementary lookup for the same service type.</param>
    /// <param name="ifAlreadyRegistered">Policy how to deal with already registered factory with same service type and key.</param>
    /// <param name="isStaticallyChecked">[performance] Confirms that service and implementation types are statically checked by compiler.</param>
    /// <returns>True if factory was added to registry, false otherwise.
    /// False may be in case of <see cref="IfAlreadyRegistered.Keep"/> setting and already existing factory.</returns>
    void Register(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered? ifAlreadyRegistered, bool isStaticallyChecked);

    /// <summary>Returns true if expected factory is registered with specified service key and type.
    /// Not provided or <c>null</c> <paramref name="serviceKey"/> means to check the <paramref name="serviceType"/> 
    /// alone with any service key.</summary>
    bool IsRegistered(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition);

    /// <summary>Removes factory with specified service type and key from registry and cache.
    /// BUT consuming services may still hold on the resolved service instance.
    /// The cache of consuming services may also hold on the unregistered service. Use `IContainer.ClearCache` to clear all cache.</summary>
    void Unregister(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition);

    /// <summary>Returns all registered service factories with their Type and optional Key.
    /// Decorator and Wrapper types are not included.</summary>
    IEnumerable<ServiceRegistrationInfo> GetServiceRegistrations();

    /// <summary>Searches for registered factories by type, and key (if specified),
    /// and by factory type (by default uses <see cref="FactoryType.Service"/>).
    /// May return empty, 1 or multiple factories.</summary>
    Factory[] GetRegisteredFactories(Type serviceType, object serviceKey, FactoryType factoryType);

    /// Puts instance into the current scope or singletons.
    //[Obsolete("me")]
    void UseInstance(Type serviceType, object instance, IfAlreadyRegistered IfAlreadyRegistered,
        bool preventDisposal, bool weaklyReferenced, object serviceKey);

    /// <summary>Puts instance created via the passed factory on demand into the current or singleton scope</summary>
    void Use(Type serviceType, FactoryDelegate factory);
  }

  /// <summary>What to do with registrations when creating the new container from the existent one.</summary>
  public enum RegistrySharing
  {
    /// <summary>Shares both registrations and resolution cache if any</summary>
    Share = 0,
    /// <summary>Clones the registrations but preserves the resolution cache</summary>
    CloneButKeepCache,
    /// <summary>Clones the registrations and drops the cache -- full reset!</summary>
    CloneAndDropCache
  }

  // public struct FactoriesWithKeys
  // {
  //     public Factory[] Factories;

  // }

  /// <summary>Combines registrator and resolver roles, plus rules and scope management.</summary>
  public interface IContainer : IRegistrator, IResolverContext
  {
    /// <summary>Rules for defining resolution/registration behavior throughout container.</summary>
    Rules Rules { get; }

    /// <summary>Represents scope bound to container itself, and not an ambient (context) thingy.</summary>
    IScope OwnCurrentScope { get; }

    // todo: @api replace with the below overload with more parameters
    /// <summary>Creates new container from the current one by specifying the listed parameters.
    /// If the null or default values are provided then the default or new values will be applied.
    /// Nothing will be inherited from the current container.
    /// If you want to inherit something you need to provide it as parameter.</summary>
    IContainer With(Rules rules, IScopeContext scopeContext, RegistrySharing registrySharing, IScope singletonScope);

    // todo: @api replace with the below overload with more parameters
    /// <summary>Creates new container from the current one by specifying the listed parameters.
    /// If the null or default values are provided then the default or new values will be applied.
    /// Nothing will be inherited from the current container. If you want to inherit something you need to provide it as parameter.</summary>
    IContainer With(IResolverContext parent, Rules rules, IScopeContext scopeContext,
        RegistrySharing registrySharing, IScope singletonScope, IScope currentScope);

    /// <summary>Creates new container from the current one by specifying the listed parameters.
    /// If the null or default values are provided then the default or new values will be applied.
    /// Nothing will be inherited from the current container. If you want to inherit something you need to provide it as parameter.</summary>
    IContainer With(IResolverContext parent, Rules rules, IScopeContext scopeContext,
        RegistrySharing registrySharing, IScope singletonScope, IScope currentScope,
        IsRegistryChangePermitted? isRegistryChangePermitted);

    // todo: @api no need for the interface definition because the it may be implemented (already) in terms of With as extension method
    /// <summary>Produces new container which prevents any further registrations.</summary>
    /// <param name="ignoreInsteadOfThrow">(optional)Controls what to do with registrations: ignore or throw exception.
    /// Throws exception by default.</param>
    /// <returns>New container preserving all current container state but disallowing registrations.</returns>
    IContainer WithNoMoreRegistrationAllowed(bool ignoreInsteadOfThrow = false);

    /// <summary>Searches for requested factory in registry, and then using <see cref="Injector.Rules.UnknownServiceResolvers"/>.</summary>
    /// <param name="request">Factory request.</param>
    /// <returns>Found factory, otherwise null if <see cref="Request.IfUnresolved"/> is set to <see cref="IfUnresolved.ReturnDefault"/>.</returns>
    Factory ResolveFactory(Request request);

    /// <summary>Searches for registered service factory and returns it, or null if not found.
    /// Will use <see cref="Injector.Rules.FactorySelector"/> if specified.</summary>
    /// <param name="request">Factory request.</param>
    /// <returns>Found factory or null.</returns>
    Factory GetServiceFactoryOrDefault(Request request);

    /// <summary>Finds all registered default and keyed service factories and returns them.
    /// It skips decorators and wrappers.</summary>
    /// <param name="serviceType">Service type to look for, may be open-generic type too.</param>
    /// <param name="bothClosedAndOpenGenerics">(optional) For generic serviceType instructs to look for
    /// both closed and open-generic registrations.</param>
    /// <returns>Enumerable of found pairs.</returns>
    /// <remarks>Returned Key item should not be null - it should be <see cref="DefaultKey.Value"/>.</remarks>
    IEnumerable<KV<object, Factory>> GetAllServiceFactories(Type serviceType, bool bothClosedAndOpenGenerics = false);

    /// <summary>The method will get all service factories registered and from the dynamic registration providers (if any) for the passed `serviceType`.
    /// The method does not try to cache the dynamic provider factories and will be calling them every time.</summary>
    KV<object, Factory>[] GetServiceRegisteredAndDynamicFactories(Type serviceType);

    /// <summary>Searches for registered wrapper factory and returns it, or null if not found.</summary>
    /// <param name="serviceType">Service type to look for.</param> <returns>Found wrapper factory or null.</returns>
    Factory GetWrapperFactoryOrDefault(Type serviceType);

    /// <summary>Returns the true if the type is wrapper</summary>
    bool IsWrapper(Type serviceType, Type openGenericServiceType = null);

    /// <summary>Returns all decorators registered for the service type.</summary> <returns>Decorator factories.</returns>
    Factory[] GetDecoratorFactoriesOrDefault(Type serviceType);

    /// <summary>Creates decorator expression: it could be either Func{TService,TService},
    /// or service expression for replacing decorators.</summary>
    /// <param name="request">Decorated service request.</param>
    /// <returns>Decorator expression.</returns>
    Expression GetDecoratorExpressionOrDefault(Request request);

    /// <summary>If <paramref name="serviceType"/> is generic type then this method checks if the type registered as generic wrapper,
    /// and recursively unwraps and returns its type argument. This type argument is the actual service type we want to find.
    /// Otherwise, method returns the input <paramref name="serviceType"/>.</summary>
    /// <param name="serviceType">Type to unwrap. Method will return early if type is not generic.</param>
    /// <param name="requiredServiceType">Required service type or null if don't care.</param>
    /// <returns>Unwrapped service type in case it corresponds to registered generic wrapper, or input type in all other cases.</returns>
    Type GetWrappedType(Type serviceType, Type requiredServiceType);

    /// <summary>Converts known items into custom expression or wraps in a constant expression.</summary>
    /// <param name="item">Item to convert.</param>
    /// <param name="itemType">(optional) Type of item, otherwise item <see cref="object.GetType()"/>.</param>
    /// <param name="throwIfStateRequired">(optional) Throws for non-primitive and not-recognized items,
    /// identifying that result expression require run-time state. For compiled expression it means closure in lambda delegate.</param>
    /// <returns>Returns constant or state access expression for added items.</returns>
    Expression GetConstantExpression(object item, Type itemType = null, bool throwIfStateRequired = false);

    /// <summary>Clears cache for specified service(s). But does not clear instances of already resolved/created singletons and scoped services!</summary>
    /// <param name="serviceType">Target service type.</param>
    /// <param name="factoryType">(optional) If not specified, clears cache for all <see cref="FactoryType"/>.</param>
    /// <param name="serviceKey">(optional) If omitted, the cache will be cleared for all registrations of <paramref name="serviceType"/>.</param>
    /// <returns>True if target service was found, false - otherwise.</returns>
    bool ClearCache(Type serviceType, FactoryType? factoryType, object serviceKey);

    /// Puts instance created via the passed factory on demand into the current or singleton scope
    new void Use(Type serviceType, FactoryDelegate factory);

    /// [Obsolete("Replaced by `Use` to put runtime data into container scopes and with `RegisterInstance` as a sugar for `RegisterDelegate(_ => instance)`")]
    new void UseInstance(Type serviceType, object instance, IfAlreadyRegistered IfAlreadyRegistered,
        bool preventDisposal, bool weaklyReferenced, object serviceKey);
  }

  /// <summary>Resolves all registered services of <typeparamref name="TService"/> type on demand,
  /// when enumerator <see cref="IEnumerator.MoveNext"/> called. If service type is not found, empty returned.</summary>
  /// <typeparam name="TService">Service type to resolve.</typeparam>
  public sealed class LazyEnumerable<TService> : IEnumerable<TService>
  {
    /// <summary>Exposes internal items enumerable.</summary>
    public readonly IEnumerable<TService> Items;

    /// <summary>Wraps lazy resolved items.</summary> <param name="items">Lazy resolved items.</param>
    public LazyEnumerable(IEnumerable<TService> items)
    {
      Items = items.ThrowIfNull();
    }

    /// <summary>Return items enumerator.</summary> 
    public IEnumerator<TService> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }

  /// <summary>Wrapper type to box service with associated arbitrary metadata object.</summary>
  /// <typeparam name="T">Service type.</typeparam>
  /// <typeparam name="TMetadata">Arbitrary metadata object type.</typeparam>
  public sealed class Meta<T, TMetadata>
  {
    /// <summary>Value or object with associated metadata.</summary>
    public readonly T Value;

    /// <summary>Associated metadata object. Could be anything.</summary>
    public readonly TMetadata Metadata;

    /// <summary>Boxes value and its associated metadata together.</summary>
    public Meta(T value, TMetadata metadata)
    {
      Value = value;
      Metadata = metadata;
    }
  }

  /// Exception that container throws in case of error. Dedicated exception type simplifies
  /// filtering or catching container relevant exceptions from client code.
#if SUPPORTS_SERIALIZABLE
  [Serializable]
#endif
  [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Not available in PCL.")]
  public class ContainerException : InvalidOperationException
  {
    /// <summary>Error code of exception, possible values are listed in <see cref="Error"/> class.</summary>
    public readonly int Error;

    /// <summary>Simplifies the access to the error name.</summary>
    public string ErrorName => Crystal.Error.NameOf(Error);

    /// <summary>Many collected exceptions</summary>
    public readonly ContainerException[] CollectedExceptions;

    /// <summary>Creates exception by wrapping <paramref name="errorCode"/> and its message,
    /// optionally with <paramref name="innerException"/> exception.</summary>
    public static ContainerException Of(ErrorCheck errorCheck, int errorCode,
        object arg0, object arg1 = null, object arg2 = null, object arg3 = null, Exception innerException = null) =>
        new ContainerException(errorCode,
            string.Format(GetMessage(errorCheck, errorCode), Print(arg0), Print(arg1), Print(arg2), Print(arg3)),
            innerException);

    /// <summary>Gets error message based on provided args.</summary> <param name="errorCheck"></param> <param name="errorCode"></param>
    protected static string GetMessage(ErrorCheck errorCheck, int errorCode) =>
        errorCode == -1 ? Throw.GetDefaultMessage(errorCheck) : Crystal.Error.Messages[errorCode];

    /// <summary>Prints argument for formatted message.</summary> <param name="arg">To print.</param> <returns>Printed string.</returns>
    protected static string Print(object arg) =>
        arg == null ? string.Empty : new StringBuilder().Print(arg).ToString();

    /// <summary>Collects many exceptions.</summary>
    public ContainerException(int error, ContainerException[] exceptions)
        : this(error, GetMessage(ErrorCheck.CollectedExceptions, error), null) => CollectedExceptions = exceptions;

    /// <summary>Creates exception with message describing cause and context of error.</summary>
    public ContainerException(int error, string message)
        : this(error, message, null) { }

    /// <summary>The optional additional exception data.</summary>
    public readonly object Details;
    private ContainerException(object details, int error, string message)
        : this(error, message, null) => Details = details;

    /// <summary>Creates exception with details object and the message.</summary>
    public static object WithDetails(object details, int error, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
    {
      throw new ContainerException(details, error,
          string.Format(GetMessage(ErrorCheck.Unspecified, error), Print(arg0), Print(arg1), Print(arg2), Print(arg3)));
    }

    /// <summary>Creates exception with message describing cause and context of error,
    /// and leading/system exception causing it.</summary>
    public ContainerException(int errorCode, string message, Exception innerException)
        : this(errorCode, message, innerException, (e, m, _) => FormatMessage(Crystal.Error.NameOf(e), m)) { }

    /// <summary>The default exception message format.</summary>
    protected static string FormatMessage(string errorName, string message) =>
        $"code: Error.{errorName};{NewLine}message: {message}";

    /// <summary>Allows the formatting of the final exception message.</summary>
    protected ContainerException(int errorCode, string message, Exception innerException,
        Func<int, string, Exception, string> formatMessage)
        : base(formatMessage(errorCode, message, innerException), innerException) => Error = errorCode;


#if SUPPORTS_SERIALIZABLE
    /// <inheritdoc />
    protected ContainerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context) { }
#endif

    /// <summary>Tries to explain the specific exception based on the passed container</summary>
    public string TryGetDetails(IContainer container)
    {
      var e = Error;
      if (e == Crystal.Error.WaitForScopedServiceIsCreatedTimeoutExpired)
      {
        var m = Message;
        var factoryId = (int)Details;
        var reg = container.GetServiceRegistrations().FirstOrDefault(r => r.Factory.FactoryID == factoryId);
        if (reg.Factory == null)
          return "Unable to get the service registration for the problematic FactoryID=" + factoryId;
        return "The service registration related to the problem is " + reg;
      }
      return string.Empty;
    }
  }

  /// <summary>Defines error codes and error messages for all DryIoc exceptions (DryIoc extensions may define their own.)</summary>
  public static class Error
  {
    private static int _errorIndex = -1;

    /// <summary>List of error messages indexed with code.</summary>
    public static readonly string[] Messages = new string[100];

#pragma warning disable 1591 // "Missing XML-comment"
    public static readonly int
        UnableToResolveUnknownService = Of(
            "Unable to resolve {0}" + NewLine +
            "Where no service registrations found" + NewLine +
            "  and no dynamic registrations found in {1} of Rules.DynamicServiceProviders" + NewLine +
            "  and nothing found in {2} of Rules.UnknownServiceResolvers"),

        UnableToResolveFromRegisteredServices = Of(
            "Unable to resolve {0}" + NewLine +
            "  with normal and dynamic registrations:" + NewLine + "{1}"),

        ExpectedSingleDefaultFactory = Of(
            "Expecting a single default registration but found many:" + NewLine + "{0}" + NewLine +
            "when resolving {1}." + NewLine +
            "Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory."),

        RegisteringImplementationNotAssignableToServiceType = Of(
            "Registering implementation type {0} is not assignable to service type {1}."),
        RegisteredFactoryMethodResultTypesIsNotAssignableToImplementationType = Of(
            "Registered factory method return type {1} should be assignable Or castable to implementation type {0} but it is not."),
        ImpossibleToRegisterOpenGenericWithRegisterDelegate = Of( // todo: @fix Improve the naming to say something about open-generic
            "Unable to register delegate factory for open-generic service {0}." + NewLine +
            "You need to specify concrete (closed) service type returned by delegate."),
        RegisteringOpenGenericImplWithNonGenericService = Of(
            "Unable to register open-generic implementation {0} with non-generic service {1}."),
        RegisteringOpenGenericServiceWithMissingTypeArgs = Of(
            "Unable to register open-generic implementation {0} because service {1} should specify all type arguments, but specifies only {2}."),
        RegisteringNotAGenericTypedefImplType = Of(
            "Unsupported registration of implementation {0} which is not a generic type definition but contains generic parameters." + NewLine +
            "Consider to register generic type definition {1} instead."),
        RegisteringNotAGenericTypedefServiceType = Of(
            "Unsupported registration of service {0} which is not a generic type definition but contains generic parameters." + NewLine +
            "Consider to register generic type definition {1} instead."),
        RegisteringNullImplementationTypeAndNoFactoryMethod = Of(
            "Registering without implementation type and without FactoryMethod to use instead."),
        RegisteringObjectTypeAsImplementationIsNotSupported = Of(
            "Registering `System.Object` type as implementation without a factory method is not supported."),
        RegisteringAbstractImplementationTypeAndNoFactoryMethod = Of(
            "Registering abstract implementation type {0} when it is should be concrete. Also there is not FactoryMethod to use instead."),
        UnableToSelectSinglePublicConstructorFromMultiple = Of(
            "Unable to select single public constructor from implementation type {0}:" + NewLine +
            "{1}"),
        UnableToSelectSinglePublicConstructorFromNone = Of(
            "Unable to select single public constructor from implementation type {0} because it does not have one."),
        NoMatchedImplementedTypesWithServiceType = Of(
            "Unable to match service with open-generic {0} implementing {1} when resolving {2}."),
        NoMatchedFactoryMethodDeclaringTypeWithServiceTypeArgs = Of(
            "Unable to match open-generic factory method Declaring type {0} with requested service type arguments <{1}> when resolving {2}."),
        NoMatchedFactoryMethodWithServiceTypeArgs = Of(
            "Unable to match open-generic factory method {0} with requested service type arguments <{1}> when resolving {2}."),
        OpenGenericFactoryMethodDeclaringTypeIsNotSupportedOnThisPlatform = Of(
            "[Specific to this .NET version] Unable to match method or constructor {0} from open-generic declaring type {1} to closed-generic type {2}, " +
            NewLine +
            "Please give the method an unique name to distinguish it from other overloads."),
        ResolvingOpenGenericServiceTypeIsNotPossible = Of(
            "Resolving open-generic service type is not possible for type: {0}."),
        RecursiveDependencyDetected = Of(
            "Recursive dependency is detected when resolving " + NewLine + "{0}."),
        ScopeIsDisposed = Of(
            "Scope {0} is disposed and scoped instances are disposed and no longer available."),
        NotFoundOpenGenericImplTypeArgInService = Of(
            "Unable to find for open-generic implementation {0} the type argument {1} when resolving {2}."),
        UnableToSelectCtor = Of(
            "Unable to get constructor of {0} using provided constructor selector when resolving {1}."),
        UnableToFindCtorWithAllResolvableArgs = Of(
            "Unable to find most resolvable constructor also including passed input arguments `{0}` " +
            NewLine + " when resolving: {1}."),
        RegisteredDelegateResultIsNotOfServiceType = Of(
            "Registered factory delegate returns service {0} is not assignable to desired service {1}."),
        NotFoundSpecifiedWritablePropertyOrField = Of(
            "Unable to find writable property or field {0} when resolving: {1}."),
        PushingToRequestWithoutFactory = Of(
            "Pushing the next request {0} into parent request not yet resolved to factory: {1}"),
        NoMatchedGenericParamConstraints = Of(
            "Open-generic service does not match with registered open-generic implementation constraints {0} when resolving: {1}."),
        GenericWrapperWithMultipleTypeArgsShouldSpecifyArgIndex = Of(
            "Generic wrapper type {0} should specify what type argument is wrapped, but it does not."),
        GenericWrapperTypeArgIndexOutOfBounds = Of(
            "Registered generic wrapper {0} specified type argument index {1} is out of type argument list."),
        DependencyHasShorterReuseLifespan = Of(
            "Dependency {0} with reuse {1} has a shorter lifespan than its parent's {2}" + NewLine +
            "If you know what you're doing you may disable this error with the rule `new Container(rules => rules.WithoutThrowIfDependencyHasShorterReuseLifespan())`."),
        WeakRefReuseWrapperGCed = Of(
            "Reused service wrapped in WeakReference is Garbage Collected and no longer available."),
        ServiceIsNotAssignableFromFactoryMethod = Of(
            "Service of {0} is not assignable from factory method {1} when resolving: {2}."),
        GotNullConstructorFromFactoryMethod = Of(
            "Got null constructor when resolving {0}"),
        UnableToRegisterDuplicateDefault = Of(
            "The default service {0} without key {1} is already registered as {2}."),
        UnableToRegisterDuplicateKey = Of(
            "Unable to register service with duplicate key '{0}': {1}" + NewLine +
            " There is already registered service with the same key {2}."),
        NoCurrentScope = Of(
            "No current scope is available: probably you are registering to, or resolving from outside of the scope. " + NewLine +
            "Current resolver context is: {0}."),
        ContainerIsDisposed = Of(
            "Container is disposed and should not be used: {0}"),
        NoMatchedScopeFound = Of(
            "Unable to find matching scope with name {0} starting from the current scope {1}."),
        NotSupportedMadeOfExpression = Of(
            "Expected expression of method call, property getter, or new statement (with optional property initializer), " +
            "but found this Made.Of expression: {0}"),
        UnexpectedFactoryMemberExpressionInMadeOf = Of(
            "Expected property getter, but found not supported `{0}` " + NewLine +
            "in Made.Of expression: `{1}`"),
        UnexpectedExpressionInsteadOfArgMethodInMadeOf = Of(
            "Expected `Arg.Of` method call to specify parameter, property or field, but found `{0}` " + NewLine +
            "in Made.Of expression: `{1}`"),
        UnexpectedExpressionInsteadOfConstantInMadeOf = Of(
            "Expected `ConstantExpression` for value of parameter, property, or field, but found `{0}` " + NewLine +
            "in Made.Of expression: `{1}`"),
        InjectedCustomValueIsOfDifferentType = Of(
            "Injected value {0} is not assignable to {1} when resolving: {2}"),
        NoConversionOperatorFoundWhenInterpretingTheConvertExpression = Of(
            "There is no explicit or implicit conversion operator found when interpreting {0} to {1} in expression: {2}"),
        StateIsRequiredToUseItem = Of(
            "Runtime state is required to inject (or use) the: {0}. " + NewLine +
            "The reason is using RegisterDelegate, Use (or UseInstance), RegisterInitializer/Disposer, or registering with non-primitive service key, or metadata." + NewLine +
            "You can convert run-time value to expression via container.With(rules => rules.WithItemToExpressionConverter(YOUR_ITEM_TO_EXPRESSION_CONVERTER))."),
        ArgValueIndexIsProvidedButNoArgValues = Of(
            "`Arg.Index` is provided but no values are passed in Made.Of expression: " + NewLine +
            "{0}"),
        ArgValueIndexIsOutOfProvidedArgValues = Of(
            "`Arg.Index` {0} is outside of provided values [{1}] in Made.Of expression: " + NewLine +
            "{2}"),
        ResolutionNeedsRequiredServiceType = Of(
            "Expecting required service type but it is not specified when resolving: {0}"),
        RegisterMappingNotFoundRegisteredService = Of(
            "When registering mapping, Container is unable to find factory of registered service type {0} and key {1}."),
        RegisterMappingUnableToSelectFromMultipleFactories = Of(
            "RegisterMapping selected more than 1 factory with provided type {0} and key {1}: {2}"),
        RegisteringInstanceNotAssignableToServiceType = Of(
            "Registered instance of type {0} is not assignable to serviceType {1}."),
        NoMoreRegistrationsAllowed = Of(
            "Container does not allow further registrations." + NewLine +
            "Attempting to register {0}{1} with implementation factory {2}."),
        NoMoreUnregistrationsAllowed = Of(
            "Container does not allow further registry modification." + NewLine +
            "Attempting to Unregister {0}{1} with factory type {2}."),
        GotNullFactoryWhenResolvingService = Of(
            "Got null factory method when resolving {0}"),
        RegisteredDisposableTransientWontBeDisposedByContainer = Of(
            "Registered Disposable Transient service {0} with key {1} registered as {2} won't be disposed by container." +
            " DryIoc does not hold reference to resolved transients, and therefore does not control their dispose." +
            " To silence this exception Register<YourService>(setup: Setup.With(allowDisposableTransient: true)) " +
            " or set the rule Container(rules => rules.WithoutThrowOnRegisteringDisposableTransient())." +
            " To enable tracking use Register<YourService>(setup: Setup.With(trackDisposableTransient: true)) " +
            " or set the rule Container(rules => rules.WithTrackingDisposableTransients())"),
        NotFoundMetaCtorWithTwoArgs = Of(
            "Expecting Meta wrapper public constructor with two arguments {0} but not found when resolving: {1}"),
        UnableToSelectFromManyRegistrationsWithMatchingMetadata = Of(
            "Unable to select from multiple registrations matching the Metadata type {0}:" + NewLine +
            "{1}" + NewLine +
            "When resolving: {2}"),
        ImplTypeIsNotSpecifiedForAutoCtorSelection = Of(
            "Implementation type is not specified when using automatic constructor selection: {0}"),
        NoImplementationForPlaceholder = Of(
            "There is no real implementation, only a placeholder for the service {0}." + NewLine +
            "Please Register the implementation with the ifAlreadyRegistered.Replace parameter to fill the placeholder."),
        UnableToFindSingletonInstance = Of(
            "Expecting the instance to be stored in singleton scope, but unable to find anything here." + NewLine +
            "Likely, you've called UseInstance from the scoped container, but resolving from another container or injecting into a singleton."),
        DecoratorShouldNotBeRegisteredWithServiceKey = Of(
            "Registering Decorator {0} with service key {1} is not supported," + NewLine +
            "because instead of decorator with the key you actually want a decorator for service registered with the key." + NewLine +
            "To apply decorator for service with the key, please use `Setup.DecoratorOf(decorateeServiceKey: \"a service key\")`"),
        PassedCtorOrMemberIsNull = Of(
            "The constructor of member info passed to `Made.Of` or `FactoryMethod.Of` is null"),
        PassedMemberIsNotStaticButInstanceFactoryIsNull = Of(
            "The member info {0} passed to `Made.Of` or `FactoryMethod.Of` is NOT static, but instance factory is not provided or null"),
        PassedMemberIsStaticButInstanceFactoryIsNotNull = Of(
            "You are passing constructor or STATIC member info {0} to `Made.Of` or `FactoryMethod.Of`, but then why are you passing factory INSTANCE: {1}"),
        UndefinedMethodWhenGettingTheSingleMethod = Of(
            "Undefined Method '{0}' in Type {1} (including non-public={2})"),
        UndefinedMethodWhenGettingMethodWithSpecifiedParameters = Of(
            "Undefined Method '{0}' in Type {1} with parameters {2}."),
        UndefinedPropertyWhenGettingProperty = Of("Undefined property {0} in type {1}"),
        UndefinedFieldWhenGettingField = Of("Undefined field {0} in type {1}"),
        UnableToFindConstructorWithArgs = Of("Unable to find a constructor in Type {0} with args: {1}"),
        UnableToFindSingleConstructor = Of(
            "Unable to find a single constructor in Type {0} (including non-public={1})"),
        DisposerTrackForDisposeError = Of("Something is {0} already."),
        NoServicesWereRegisteredByRegisterMany = Of(
            "No service types were discovered in `RegisterMany` (or in `RegisterInstanceMany`) for the specified implementation types: " + NewLine +
            "[{0}]" + NewLine +
            "Maybe you missed the implementation or service type(s), " +
            "e.g. provided only abstract or compiler-generated implementation types, " +
            "or specified a wrong `serviceTypeCondition`," +
            "or did not specify to use `nonPublicServiceTypes`, etc."),
        FoundNoRootsToValidate = Of(
            "No roots to Validate found. Check the `condition` passed to Validate method for container: {0}" + NewLine +
            "You may also examine all container registrations via `container.container.GetServiceRegistrations()` method."),
        NoServiceTypesToValidate = Of(
            "The `serviceTypes` passed to Validate method is null or empty. Please pass the type(s) you want to Validate."),
        ValidateFoundErrors = Of(
            "Validate found the errors, please check the ContainerException.CollectedExceptions for details."),
        UnableToInterpretTheNestedLambda = Of(
            "Unable to interpret the nested lambda with Body:" + NewLine +
            "{0}"),
        WaitForScopedServiceIsCreatedTimeoutExpired = Of(
            "DryIoc has waited for the creation of the scoped or singleton service by the \"other party\" for the {1} ticks without the completion. " + NewLine +
            "You may call `exception.TryGetDetails(container)` to get the details of the problematic service registration." + NewLine +
            "The error means that either the \"other party\" is the parallel thread which has started but is unable to finish the creation of the service in the provided amount of time. " + NewLine +
            "Or more likely the \"other party\"  is the same thread and there is an undetected recursive dependency or " + NewLine +
            "the scoped service creation is failed with the exception and the exception was catched but you are trying to resolve the failed service again. " + NewLine +
            "For all those reasons DryIoc has a timeout to prevent the infinite waiting. " + NewLine +
            $"You may change the default timeout via `Scope.{nameof(Scope.WaitForScopedServiceIsCreatedTimeoutTicks)}=NewNumberOfTicks`");

#pragma warning restore 1591 // "Missing XML-comment"

    private static int Of(string message)
    {
      var errorIndex = Interlocked.Increment(ref _errorIndex);
      Messages[errorIndex] = message;
      return errorIndex;
    }

    /// <summary>Returns the name of error with the provided error code.</summary>
    public static string NameOf(int error) =>
        error == -1 ? "ErrorCheck" :
        typeof(Error).GetTypeInfo().DeclaredFields
            .Where(f => f.FieldType == typeof(int)).Where((_, i) => i == error + 1)
            .FirstOrDefault()?.Name;

    static Error()
    {
      Throw.GetMatchedException = ContainerException.Of;
    }
  }

  /// <summary>Checked error condition, possible error sources.</summary>
  public enum ErrorCheck
  {
    /// <summary>Unspecified, just throw.</summary>
    Unspecified,
    /// <summary>Predicate evaluated to false.</summary>
    InvalidCondition,
    /// <summary>Checked object is null.</summary>
    IsNull,
    /// <summary>Checked object is of unexpected type.</summary>
    IsNotOfType,
    /// <summary>Checked type is not assignable to expected type</summary>
    TypeIsNotOfType,
    /// <summary>Invoked operation throws, it is source of inner exception.</summary>
    OperationThrows,
    /// <summary>Just stores many collected exceptions.</summary>
    CollectedExceptions,
  }

  /// <summary>Enables more clean error message formatting and a bit of code contracts.</summary>
  public static class Throw
  {
    private static string[] CreateDefaultMessages()
    {
      var messages = new string[(int)ErrorCheck.CollectedExceptions + 1];
      messages[(int)ErrorCheck.Unspecified] = "The error reason is unspecified, which is bad thing.";
      messages[(int)ErrorCheck.InvalidCondition] = "Argument {0} of type {1} has invalid condition.";
      messages[(int)ErrorCheck.IsNull] = "Argument of type {0} is null.";
      messages[(int)ErrorCheck.IsNotOfType] = "Argument {0} is not of type {1}.";
      messages[(int)ErrorCheck.TypeIsNotOfType] = "Type argument {0} is not assignable from type {1}.";
      messages[(int)ErrorCheck.OperationThrows] = "Invoked operation throws the inner exception {0}.";
      messages[(int)ErrorCheck.CollectedExceptions] = "Please check the `ContainerException.CollectedExceptions` for the details";
      return messages;
    }

    private static readonly string[] _defaultMessages = CreateDefaultMessages();

    /// <summary>Returns the default message specified for <see cref="ErrorCheck"/> code.</summary>
    public static string GetDefaultMessage(ErrorCheck error) =>
        _defaultMessages[(int)error];

    /// <summary>Declares mapping between <see cref="ErrorCheck"/> type and <paramref name="error"/> code to specific <see cref="Exception"/>.</summary>
    public delegate Exception GetMatchedExceptionHandler(ErrorCheck errorCheck, int error, object arg0, object arg1, object arg2, object arg3, Exception inner);

    /// <summary>Returns matched exception for error check and error code.</summary>
    public static GetMatchedExceptionHandler GetMatchedException = ContainerException.Of;

    /// <summary>Throws matched exception with provided error code if throw condition is true.</summary>
    public static void If(bool throwCondition, int error = -1, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
    {
      if (throwCondition)
        throw GetMatchedException(ErrorCheck.InvalidCondition, error, arg0, arg1, arg2, arg3, null);
    }

    /// <summary>Throws matched exception with provided error code if throw condition is true.
    /// Otherwise returns source <paramref name="arg0"/>.</summary>
    public static T ThrowIf<T>(this T arg0, bool throwCondition, int error = -1, object arg1 = null, object arg2 = null, object arg3 = null)
    {
      if (!throwCondition) return arg0;
      throw GetMatchedException(ErrorCheck.InvalidCondition, error, arg0, arg1, arg2, arg3, null);
    }

    /// <summary>Throws exception if <paramref name="arg"/> is null, otherwise returns <paramref name="arg"/>.</summary>
    public static T ThrowIfNull<T>(this T arg, int error = -1, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
        where T : class
    {
      if (arg != null) return arg;
      throw GetMatchedException(ErrorCheck.IsNull, error, arg0 ?? typeof(T), arg1, arg2, arg3, null);
    }

    /// <summary>Throws exception if <paramref name="arg0"/> is not assignable to type specified by <paramref name="arg1"/>,
    /// otherwise just returns <paramref name="arg0"/>.</summary>
    public static T ThrowIfNotInstanceOf<T>(this T arg0, Type arg1, int error = -1, object arg2 = null, object arg3 = null)
        where T : class
    {
      var arg1ti = arg1.GetTypeInfo();
      if (arg0 == null && (!arg1ti.IsValueType || arg1ti.IsGenericType && arg1.GetGenericTypeDefinition() == typeof(Nullable<>)) ||
          arg1ti.IsAssignableFrom(arg0.GetType().GetTypeInfo()))
        return arg0;
      throw GetMatchedException(ErrorCheck.IsNotOfType, error, arg0, arg1, arg2, arg3, null);
    }

    /// <summary>Throws if <paramref name="arg0"/> is not assignable from <paramref name="arg1"/>.</summary>
    public static Type ThrowIfNotImplementedBy(this Type arg0, Type arg1, int error = -1, object arg2 = null, object arg3 = null)
    {
      if (arg1.IsAssignableTo(arg0)) return arg0;
      throw GetMatchedException(ErrorCheck.TypeIsNotOfType, error, arg0, arg1, arg2, arg3, null);
    }

    /// <summary>Invokes <paramref name="operation"/> and in case of <typeparamref name="TEx"/> re-throws it as inner-exception.</summary>
    public static T IfThrows<TEx, T>(Func<T> operation, bool throwCondition, int error, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null) where TEx : Exception
    {
      try
      {
        return operation();
      }
      catch (TEx ex)
      {
        if (throwCondition)
          throw GetMatchedException(ErrorCheck.OperationThrows, error, arg0, arg1, arg2, arg3, ex);
        return default(T);
      }
    }

    /// <summary>Just throws the exception with the <paramref name="error"/> code.</summary>
    public static object It(int error, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
    {
      throw GetMatchedException(ErrorCheck.Unspecified, error, arg0, arg1, arg2, arg3, null);
    }

    /// <summary>Just throws the exception with the <paramref name="error"/> code.</summary>
    public static object WithDetails(object details, int error, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null) =>
        ContainerException.WithDetails(details, error, arg0, arg1, arg2, arg3);

    /// <summary>Throws <paramref name="error"/> instead of returning value of <typeparamref name="T"/>.
    /// Supposed to be used in expression that require some return value.</summary>
    public static T For<T>(int error, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
    {
      throw GetMatchedException(ErrorCheck.Unspecified, error, arg0, arg1, arg2, arg3, null);
    }

    /// <summary>Throws if contidion is true, otherwise returns the `default(T)` value</summary>
    public static T For<T>(bool throwCondition, int error,
        object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
    {
      if (!throwCondition) return default(T);
      throw GetMatchedException(ErrorCheck.Unspecified, error, arg0, arg1, arg2, arg3, null);
    }

    /// <summary>Throws the one with manyh collected exceptions</summary>
    public static void Many(int error, params ContainerException[] errors) =>
        throw new ContainerException(error, errors);
  }

  /// <summary>Called from the generated code to check if WeakReference.Value is GCed.</summary>
  public static class ThrowInGeneratedCode
  {
    /// <summary>Throws if the object is null.</summary>
    public static object WeakRefReuseWrapperGCed(this object obj)
    {
      if (obj == null) Throw.It(Error.WeakRefReuseWrapperGCed);
      return obj;
    }

    internal static readonly MethodInfo WeakRefReuseWrapperGCedMethod =
        typeof(ThrowInGeneratedCode).GetTypeInfo().GetDeclaredMethod(nameof(WeakRefReuseWrapperGCed));
    internal static readonly PropertyInfo WeakReferenceValueProperty =
        typeof(WeakReference).Property(nameof(WeakReference.Target));
    internal static readonly ConstructorInfo WeakReferenceCtor =
        typeof(WeakReference).Constructor(typeof(object));
  }

  /// <summary>Contains helper methods to work with Type: for instance to find Type implemented base types and interfaces, etc.</summary>
  public static class ReflectionTools
  {
#if SUPPORTS_DELEGATE_METHOD
    private static Lazy<Action<Exception>> _preserveExceptionStackTraceAction = new Lazy<Action<Exception>>(() =>
        typeof(Exception).GetSingleMethodOrNull("InternalPreserveStackTrace", true)
        ?.To(x => x.CreateDelegate(typeof(Action<Exception>)).To<Action<Exception>>()));

    /// <summary>Preserves the stack trace before re-throwing.</summary>
    public static Exception TryRethrowWithPreservedStackTrace(this Exception ex)
    {
      _preserveExceptionStackTraceAction.Value?.Invoke(ex);
      return ex;
    }
#else
        /// <summary>Preserves the stack trace before re-throwing.</summary>
        public static Exception TryRethrowWithPreservedStackTrace(this Exception ex) => ex;
#endif

    /// <summary>Flags for <see cref="GetImplementedTypes"/> method.</summary>
    [Flags]
    public enum AsImplementedType
    {
      /// <summary>Include nor object not source type.</summary>
      None = 0,
      /// <summary>Include source type to list of implemented types.</summary>
      SourceType = 1,
      /// <summary>Include <see cref="System.Object"/> type to list of implemented types.</summary>
      ObjectType = 2
    }

    /// <summary>Returns all interfaces and all base types (in that order) implemented by <paramref name="sourceType"/>.
    /// Specify <paramref name="asImplementedType"/> to include <paramref name="sourceType"/> itself as first item and
    /// <see cref="object"/> type as the last item.</summary>
    public static Type[] GetImplementedTypes(this Type sourceType, AsImplementedType asImplementedType = AsImplementedType.None)
    {
      Type[] results;

      var interfaces = sourceType.GetImplementedInterfaces();
      var interfaceStartIndex = (asImplementedType & AsImplementedType.SourceType) == 0 ? 0 : 1;
      var includingObjectType = (asImplementedType & AsImplementedType.ObjectType) == 0 ? 0 : 1;
      var sourcePlusInterfaceCount = interfaceStartIndex + interfaces.Length;

      var baseType = sourceType.GetTypeInfo().BaseType;
      if (baseType == null || baseType == typeof(object))
        results = new Type[sourcePlusInterfaceCount + includingObjectType];
      else
      {
        List<Type> baseBaseTypes = null;
        for (var bb = baseType.GetTypeInfo().BaseType; bb != null && bb != typeof(object); bb = bb.GetTypeInfo().BaseType)
          (baseBaseTypes ?? (baseBaseTypes = new List<Type>(2))).Add(bb);

        if (baseBaseTypes == null)
          results = new Type[sourcePlusInterfaceCount + includingObjectType + 1];
        else
        {
          results = new Type[sourcePlusInterfaceCount + baseBaseTypes.Count + includingObjectType + 1];
          baseBaseTypes.CopyTo(results, sourcePlusInterfaceCount + 1);
        }

        results[sourcePlusInterfaceCount] = baseType;
      }

      if (interfaces.Length == 1)
        results[interfaceStartIndex] = interfaces[0];
      else if (interfaces.Length > 1)
        Array.Copy(interfaces, 0, results, interfaceStartIndex, interfaces.Length);

      if (interfaceStartIndex == 1)
        results[0] = sourceType;
      if (includingObjectType == 1)
        results[results.Length - 1] = typeof(object);

      return results;
    }

    /// <summary>Gets a collection of the interfaces implemented by the current type and its base types.</summary>
    public static Type[] GetImplementedInterfaces(this Type type) =>
        type.GetTypeInfo().ImplementedInterfaces.ToArrayOrSelf();

    /// <summary>Gets all declared and if specified, the base members too.</summary>
    public static IEnumerable<MemberInfo> GetAllMembers(this Type type, bool includeBase = false) =>
        type.GetMembers(t =>
            t.DeclaredMethods.Cast<MemberInfo>().Concat(
            t.DeclaredProperties.Cast<MemberInfo>().Concat(
            t.DeclaredFields.Cast<MemberInfo>())),
            includeBase);

    /// <summary>Returns true if the <paramref name="openGenericType"/> contains all generic parameters
    /// from <paramref name="genericParameters"/>.</summary>
    public static bool ContainsAllGenericTypeParameters(this Type openGenericType, Type[] genericParameters)
    {
      if (!openGenericType.IsOpenGeneric())
        return false;

      var matchedParams = new Type[genericParameters.Length];
      Array.Copy(genericParameters, 0, matchedParams, 0, genericParameters.Length);

      ClearGenericParametersReferencedInConstraints(matchedParams);
      ClearMatchesFoundInGenericParameters(matchedParams, openGenericType.GetGenericParamsAndArgs());

      for (var i = 0; i < matchedParams.Length; i++)
        if (matchedParams[i] != null)
          return false;
      return true;
    }

    /// <summary>Where the `T` should be either Type or MethodInfo</summary>
    internal static T TryCloseGenericTypeOrMethod<T>(this T openGenericTypeOrMethod,
        Type[] typeArgs, Func<T, Type[], T> closeGeneric, bool throwCondition, int error, Request r)
    {
      try
      {
        return closeGeneric(openGenericTypeOrMethod, typeArgs);
      }
      catch (ArgumentException ex)
      {
        if (throwCondition)
          throw Throw.GetMatchedException(ErrorCheck.OperationThrows, error, openGenericTypeOrMethod, r, null, null, ex);
        return default(T);
      }
    }

    /// <summary>Returns true if class is compiler generated. Checking for CompilerGeneratedAttribute
    /// is not enough, because this attribute is not applied for classes generated from "async/await".</summary>
    public static bool IsCompilerGenerated(this Type type) =>
        type.GetTypeInfo().GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false).Any();

    /// <summary>Returns true if type is generic.</summary>
    public static bool IsGeneric(this Type type) =>
        type.GetTypeInfo().IsGenericType;

    /// <summary>Returns true if type is generic type definition (open type).</summary>
    public static bool IsGenericDefinition(this Type type) =>
        type.GetTypeInfo().IsGenericTypeDefinition;

    /// <summary>Returns true if type is closed generic: does not have open generic parameters, only closed/concrete ones.</summary>
    public static bool IsClosedGeneric(this Type type)
    {
      var typeInfo = type.GetTypeInfo();
      return typeInfo.IsGenericType && !typeInfo.ContainsGenericParameters;
    }

    /// <summary>Returns true if type if open generic: contains at list one open generic parameter. Could be
    /// generic type definition as well.</summary>
    public static bool IsOpenGeneric(this Type type)
    {
      var typeInfo = type.GetTypeInfo();
      return typeInfo.IsGenericType && typeInfo.ContainsGenericParameters;
    }

    /// <summary>Returns generic type definition if type is generic and null otherwise.</summary>
    public static Type GetGenericDefinitionOrNull(this Type type) =>
        type != null && type.GetTypeInfo().IsGenericType ? type.GetGenericTypeDefinition() : null;

    /// <summary>Returns generic type parameters and arguments in order they specified. If type is not generic, returns empty array.</summary>
    public static Type[] GetGenericParamsAndArgs(this Type type)
    {
      var ti = type.GetTypeInfo();
      return ti.IsGenericTypeDefinition ? ti.GenericTypeParameters : ti.GenericTypeArguments;
    }

    /// <summary>Returns array of interface and base class constraints for provider generic parameter type.</summary>
    public static Type[] GetGenericParamConstraints(this Type type) =>
        type.GetTypeInfo().GetGenericParameterConstraints();

    /// <summary>If type is array returns is element type, otherwise returns null.</summary>
    /// <param name="type">Source type.</param> <returns>Array element type or null.</returns>
    public static Type GetArrayElementTypeOrNull(this Type type)
    {
      var typeInfo = type.GetTypeInfo();
      return typeInfo.IsArray ? typeInfo.GetElementType() : null;
    }

    /// <summary>Return base type or null, if not exist (the case for only for object type).</summary>
    public static Type GetBaseType(this Type type) =>
        type.GetTypeInfo().BaseType;

    /// <summary>Checks if type is public or nested public in public type.</summary>
    public static bool IsPublicOrNestedPublic(this Type type)
    {
      var ti = type.GetTypeInfo();
      return ti.IsPublic || ti.IsNestedPublic && ti.DeclaringType.IsPublicOrNestedPublic();
    }

    /// <summary>Returns true if type is class.</summary>
    public static bool IsClass(this Type type) =>
        type.GetTypeInfo().IsClass;

    /// <summary>Returns true if type is value type.</summary>
    public static bool IsValueType(this Type type) =>
        type.GetTypeInfo().IsValueType;

    /// <summary>Returns true if type is interface.</summary>
    public static bool IsInterface(this Type type) =>
        type.GetTypeInfo().IsInterface;

    /// <summary>Returns true if type if abstract or interface.</summary>
    public static bool IsAbstract(this Type type) =>
        type.GetTypeInfo().IsAbstract;

    /// <summary>Returns true if type is static.</summary>
    public static bool IsStatic(this Type type)
    {
      var typeInfo = type.GetTypeInfo();
      return typeInfo.IsAbstract && typeInfo.IsSealed;
    }

    /// <summary>Returns true if type is enum type.</summary>
    public static bool IsEnum(this Type type) =>
        type.GetTypeInfo().IsEnum;

    /// <summary>Returns true if type can be casted with conversion operators.</summary>
    public static bool HasConversionOperatorTo(this Type sourceType, Type targetType) =>
        (sourceType.FindConvertOperator(sourceType, targetType) ??
         targetType.FindConvertOperator(sourceType, targetType)) != null;

    /// Returns `target source.op_(Explicit|Implicit)(source)` or null if not found
    public static MethodInfo GetSourceConversionOperatorToTarget(this Type sourceType, Type targetType) =>
        sourceType.FindConvertOperator(sourceType, targetType);

    /// Returns `target target.op_(Explicit|Implicit)(source)` or null if not found
    public static MethodInfo GetTargetConversionOperatorFromSource(this Type sourceType, Type targetType) =>
        targetType.FindConvertOperator(sourceType, targetType);

    internal static MethodInfo FindConvertOperator(this Type type, Type sourceType, Type targetType)
    {
      var methods = type.GetTypeInfo().DeclaredMethods.ToArrayOrSelf();
      for (var i = 0; i < methods.Length; i++)
      {
        var m = methods[i];
        if (m.IsStatic && m.IsSpecialName && m.ReturnType == targetType)
        {
          var n = m.Name;
          // n == "op_Implicit" || n == "op_Explicit"
          if (n.Length == 11 &&
              n[2] == '_' && n[5] == 'p' && n[6] == 'l' && n[7] == 'i' && n[8] == 'c' && n[9] == 'i' && n[10] == 't' &&
              m.GetParameters()[0].ParameterType == sourceType)
            return m;
        }
      }

      return null;
    }

    /// <summary>Returns true if type is assignable to <paramref name="other"/> type.</summary>
    public static bool IsAssignableTo(this Type type, Type other) =>
        type != null && other != null && other.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());

    /// <summary>Returns true if type is assignable to <typeparamref name="T"/> type.</summary>
    public static bool IsAssignableTo<T>(this Type type) =>
        type != null && typeof(T).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());

    /// <summary>`to` should be the closed-generic type</summary>
    public static bool IsAssignableVariantGenericTypeFrom(this Type to, Type from) =>
        from != to && from.IsGeneric() && from.GetGenericTypeDefinition() == to.GetGenericTypeDefinition() &&
        to.GetTypeInfo().IsAssignableFrom(from.GetTypeInfo());

    /// <summary>Returns true if type of <paramref name="obj"/> is assignable to source <paramref name="type"/>.</summary>
    public static bool IsTypeOf(this Type type, object obj) =>
        obj != null && obj.GetType().IsAssignableTo(type);

    /// <summary>Returns true if provided type IsPrimitive in .Net terms, or enum, or string,
    /// or array of primitives if <paramref name="orArrayOfPrimitives"/> is true.</summary>
    public static bool IsPrimitive(this Type type, bool orArrayOfPrimitives = false)
    {
      var typeInfo = type.GetTypeInfo();
      return typeInfo.IsPrimitive || typeInfo.IsEnum || type == typeof(string)
          || orArrayOfPrimitives && typeInfo.IsArray && typeInfo.GetElementType().IsPrimitive(true);
    }

    /// <summary>Returns all attributes defined on <paramref name="type"/>.</summary>
    public static Attribute[] GetAttributes(this Type type, Type attributeType = null, bool inherit = false) =>
        type.GetTypeInfo().GetCustomAttributes(attributeType ?? typeof(Attribute), inherit)
            // ReSharper disable once RedundantEnumerableCastCall
            .Cast<Attribute>() // required in .NET 4.5
            .ToArrayOrSelf();

    /// <summary>Recursive method to enumerate all input type and its base types for specific details.
    /// Details are returned by <paramref name="getMembers"/> delegate.</summary>
    public static IEnumerable<TMember> GetMembers<TMember>(
        this Type type, Func<TypeInfo, IEnumerable<TMember>> getMembers, bool includeBase = false)
    {
      var typeInfo = type.GetTypeInfo();
      var members = getMembers(typeInfo);
      if (!includeBase || typeInfo.BaseType == null || typeInfo.BaseType == typeof(object))
        return members;
      return members.Append(typeInfo.BaseType.GetMembers(getMembers, true));
    }

    /// <summary>Returns all public instance constructors for the type</summary>
    public static IEnumerable<ConstructorInfo> PublicConstructors(this Type type)
    {
      foreach (var x in type.GetTypeInfo().DeclaredConstructors)
        if (x.IsPublic && !x.IsStatic)
          yield return x;
    }

    /// <summary>Returns all public instance constructors for the type</summary>
    public static IEnumerable<ConstructorInfo> PublicAndInternalConstructors(this Type type)
    {
      foreach (var x in type.GetTypeInfo().DeclaredConstructors)
        if (!x.IsPrivate && !x.IsStatic)
          yield return x;
    }

    /// <summary>Enumerates all constructors from input type.</summary>
    public static IEnumerable<ConstructorInfo> Constructors(this Type type,
        bool includeNonPublic = false, bool includeStatic = false)
    {
      var ctors = type.GetTypeInfo().DeclaredConstructors.ToArrayOrSelf();
      if (ctors.Length == 0)
        return ctors;

      var ctor0 = ctors[0];
      var skip0 = !includeNonPublic && !ctor0.IsPublic || !includeStatic && ctor0.IsStatic;
      if (ctors.Length == 1)
        return skip0 ? ArrayTools.Empty<ConstructorInfo>() : ctors;

      if (ctors.Length == 2)
      {
        var ctor1 = ctors[1];
        var skip1 = !includeNonPublic && !ctor1.IsPublic || !includeStatic && ctor1.IsStatic;
        if (skip0 && skip1)
          return ArrayTools.Empty<ConstructorInfo>();
        if (skip0)
          return new[] { ctor1 };
        if (skip1)
          return new[] { ctor0 };
        return ctors;
      }

      if (!includeNonPublic && !includeStatic)
        return ctors.Match(x => !x.IsStatic && x.IsPublic);
      if (!includeNonPublic)
        return ctors.Match(x => x.IsPublic);
      if (!includeStatic)
        return ctors.Match(x => !x.IsStatic);
      return ctors;
    }

    /// <summary>Searches and returns the first constructor by its signature, e.g. with the same number of parameters of the same type.</summary>
    public static ConstructorInfo GetConstructorOrNull(this Type type, bool includeNonPublic = false, params Type[] args)
    {
      var argsLength = args.Length;
      var ctors = Constructors(type, includeNonPublic, includeStatic: false).ToArrayOrSelf();
      for (var c = 0; c < ctors.Length; c++)
      {
        var ctor = ctors[c];
        var ctorParams = ctor.GetParameters();
        if (ctorParams.Length == argsLength)
        {
          var i = 0;
          for (; i < argsLength; ++i)
          {
            var paramType = ctorParams[i].ParameterType;
            if (paramType != args[i] && paramType.GetGenericDefinitionOrNull() != args[i])
              break;
          }

          if (i == argsLength)
            return ctor;
        }
      }

      return null;
    }

    /// <summary>Searches and returns constructor by its signature.</summary>
    public static ConstructorInfo GetConstructorOrNull(this Type type, params Type[] args) =>
        type.GetConstructorOrNull(true, args);

    /// <summary>Searches and returns constructor by its signature, or throws if not found</summary>
    public static ConstructorInfo Constructor(this Type type, params Type[] args) =>
        type.GetConstructorOrNull(includeNonPublic: true, args: args).ThrowIfNull(Error.UnableToFindConstructorWithArgs, type, args);

    /// <summary>Returns single constructor otherwise (if no constructor or more than one) returns null.</summary>
    public static ConstructorInfo GetSingleConstructorOrNull(this Type type, bool includeNonPublic = false)
    {
      ConstructorInfo ctor = null;
      var ctors = Constructors(type, includeNonPublic, includeStatic: false).ToArrayOrSelf();
      for (var i = 0; i < ctors.Length; i++)
      {
        var x = ctors[i];
        if (ctor != null) // if multiple constructors
          return null;
        if (includeNonPublic || x.IsPublic)
          ctor = x;
      }

      return ctor;
    }

    /// <summary>Returns single constructor otherwise (if no or more than one) throws an exception</summary>
    public static ConstructorInfo SingleConstructor(this Type type, bool includeNonPublic = false) =>
        type.GetSingleConstructorOrNull(includeNonPublic).ThrowIfNull(Error.UnableToFindSingleConstructor, type, includeNonPublic);

    /// <summary>Looks up for single declared method with the specified name. Returns null if method is not found.</summary>
    public static MethodInfo GetSingleMethodOrNull(this Type type, string name, bool includeNonPublic = false)
    {
      if (includeNonPublic)
      {
        foreach (var method in type.GetTypeInfo().DeclaredMethods)
          if (method.Name == name)
            return method;
      }
      else
      {
        foreach (var method in type.GetTypeInfo().DeclaredMethods)
          if (method.IsPublic && method.Name == name)
            return method;
      }

      return null;
    }

    /// <summary>Looks for single declared (not inherited) method by name, and throws if not found.</summary>
    public static MethodInfo SingleMethod(this Type type, string name, bool includeNonPublic = false) =>
        type.GetSingleMethodOrNull(name, includeNonPublic).ThrowIfNull(
            Error.UndefinedMethodWhenGettingTheSingleMethod, name, type, includeNonPublic);

    /// <summary>Looks up for method with and specified parameter types.</summary>
    public static MethodInfo Method(this Type type, string name, params Type[] args) =>
        type.GetMethodOrNull(name, args).ThrowIfNull(
            Error.UndefinedMethodWhenGettingMethodWithSpecifiedParameters, name, type, args);

    /// <summary>Looks up for method with and specified parameter types.</summary>
    public static MethodInfo GetMethodOrNull(this Type type, string name, params Type[] paramTypes)
    {
      var pTypesCount = paramTypes.Length;
      var methods = type.GetTypeInfo().DeclaredMethods.ToArrayOrSelf();
      foreach (var method in methods)
      {
        if (method.Name == name)
        {
          var ps = method.GetParameters();
          if (ps.Length == pTypesCount)
          {
            var p = 0;
            for (; p < pTypesCount; ++p)
            {
              var paramType = ps[p].ParameterType;
              if (paramType != paramTypes[p] && paramType.GetGenericDefinitionOrNull() != paramTypes[p])
                break;
            }

            if (p == pTypesCount)
              return method;
          }
        }
      }

      return null;
    }

    /// <summary>Returns property by name, including inherited. Or null if not found.</summary>
    public static PropertyInfo Property(this Type type, string name, bool includeBase = false) =>
        type.GetPropertyOrNull(name, includeBase).ThrowIfNull(Error.UndefinedPropertyWhenGettingProperty, name, type);

    /// <summary>Returns property by name, including inherited. Or null if not found.</summary>
    public static PropertyInfo GetPropertyOrNull(this Type type, string name, bool includeBase = false)
    {
      var props = type.GetTypeInfo().DeclaredProperties.ToArrayOrSelf();
      for (var i = 0; i < props.Length; i++)
      {
        var p = props[i];
        if (p.Name == name)
          return p;
      }

      return !includeBase ? null : type.GetTypeInfo().BaseType?.GetPropertyOrNull(name, includeBase);
    }

    /// <summary>Returns field by name, including inherited. Or null if not found.</summary>
    public static FieldInfo Field(this Type type, string name, bool includeBase = false) =>
        type.GetFieldOrNull(name, includeBase).ThrowIfNull(Error.UndefinedFieldWhenGettingField, name, type);

    /// <summary>Returns field by name, including inherited. Or null if not found.</summary>
    public static FieldInfo GetFieldOrNull(this Type type, string name, bool includeBase = false)
    {
      var fields = type.GetTypeInfo().DeclaredFields.ToArrayOrSelf();
      for (var i = 0; i < fields.Length; i++)
      {
        var f = fields[i];
        if (f.Name == name)
          return f;
      }

      return !includeBase ? null : type.GetTypeInfo().BaseType?.GetFieldOrNull(name, includeBase);
    }

    /// <summary>Returns type assembly.</summary>
    public static Assembly GetAssembly(this Type type) => type.GetTypeInfo().Assembly;

    /// <summary>Is <c>true</c> for interface declared property explicitly implemented, e.g. <c>IInterface.Prop</c></summary>
    public static bool IsExplicitlyImplemented(this PropertyInfo property) => property.Name.Contains(".");

    /// <summary>Returns true if member is static, otherwise returns false.</summary>
    public static bool IsStatic(this MemberInfo member) =>
          member is MethodInfo method ? method.IsStatic
        : member is FieldInfo field ? field.IsStatic
        : member is PropertyInfo prop && !prop.IsExplicitlyImplemented() && prop.IsStatic(true);

    /// Find if property is static
    public static bool IsStatic(this PropertyInfo property, bool includeNonPublic = false)
    {
      // e.g.: set_Blah or get_Blah
      var propName = property.Name;
      var methods = property.DeclaringType.GetTypeInfo().DeclaredMethods.ToArrayOrSelf();
      for (var index = 0; index < methods.Length; index++)
      {
        var m = methods[index];
        if (m.IsSpecialName && (includeNonPublic || m.IsPublic))
        {
          var name = m.Name;
          var nameLength = name.Length;
          if (nameLength > 4 && name[3] == '_' && nameLength - 4 == propName.Length)
          {
            var i = 4;
            for (var j = 0; i < nameLength; i++, j++)
              if (name[i] != propName[j])
                break;
            if (i == nameLength)
              return m.IsStatic;
          }
        }
      }

      return false;
    }

    /// <summary>Return either <see cref="PropertyInfo.PropertyType"/>, or <see cref="FieldInfo.FieldType"/>, 
    /// <see cref="MethodInfo.ReturnType"/>.</summary>
    public static Type GetReturnTypeOrDefault(this MemberInfo member) =>
        member is ConstructorInfo ? member.DeclaringType
        : (member as MethodInfo)?.ReturnType
        ?? (member as PropertyInfo)?.PropertyType
        ?? (member as FieldInfo)?.FieldType;

    /// <summary>Returns true if field is backing field for property.</summary>
    public static bool IsBackingField(this FieldInfo field) =>
        field.Name[0] == '<';

    /// <summary>Returns true if property is indexer: aka this[].</summary>
    public static bool IsIndexer(this PropertyInfo property) =>
        property.GetIndexParameters().Length != 0;

    /// <summary>Returns true if type is generated type of hoisted closure.</summary>
    public static bool IsClosureType(this Type type) =>
        type.Name.Contains("<>c__DisplayClass");

    /// <summary>Returns attributes defined for the member/method.</summary>
    public static IEnumerable<Attribute> GetAttributes(this MemberInfo member, Type attributeType = null, bool inherit = false) =>
        member.GetCustomAttributes(attributeType ?? typeof(Attribute), inherit).Cast<Attribute>();

    /// <summary>Returns attributes defined for parameter.</summary>
    public static IEnumerable<Attribute> GetAttributes(this ParameterInfo parameter, Type attributeType = null, bool inherit = false) =>
        parameter.GetCustomAttributes(attributeType ?? typeof(Attribute), inherit).Cast<Attribute>();

    /// <summary>Get types from assembly that are loaded successfully.
    /// Hacks the <see cref="ReflectionTypeLoadException"/>to get failing to load types metadata.</summary>
    public static Type[] GetLoadedTypes(this Assembly assembly)
    {
      try
      {
        return Portable.GetAssemblyTypes(assembly).ToArrayOrSelf();
      }
      catch (ReflectionTypeLoadException ex)
      {
        return ex.Types.Where(type => type != null).ToArray();
      }
    }

    private static void ClearGenericParametersReferencedInConstraints(Type[] genericParams)
    {
      for (var i = 0; i < genericParams.Length; i++)
      {
        var genericParam = genericParams[i];
        if (genericParam == null)
          continue;

        var genericConstraints = genericParam.GetGenericParamConstraints();
        for (var j = 0; j < genericConstraints.Length; j++)
        {
          var genericConstraint = genericConstraints[j];
          if (genericConstraint.IsOpenGeneric())
          {
            var constraintGenericParams = genericConstraint.GetGenericParamsAndArgs();
            for (var k = 0; k < constraintGenericParams.Length; k++)
            {
              var constraintGenericParam = constraintGenericParams[k];
              if (constraintGenericParam != genericParam)
              {
                for (var g = 0; g < genericParams.Length; ++g)
                  if (genericParams[g] == constraintGenericParam)
                  {
                    genericParams[g] = null; // match
                    break;
                  }
              }
            }
          }
        }
      }
    }

    private static void ClearMatchesFoundInGenericParameters(Type[] matchedParams, Type[] genericParams)
    {
      for (var i = 0; i < genericParams.Length; i++)
      {
        var genericParam = genericParams[i];
        if (genericParam.IsGenericParameter)
        {
          for (var j = 0; j < matchedParams.Length; ++j)
            if (matchedParams[j] == genericParam)
            {
              matchedParams[j] = null; // match
              break;
            }
        }
        else if (genericParam.IsOpenGeneric())
          ClearMatchesFoundInGenericParameters(matchedParams, genericParam.GetGenericParamsAndArgs());
      }
    }

    internal static T GetDefault<T>() => default(T);
    internal static readonly MethodInfo GetDefaultMethod =
        typeof(ReflectionTools).SingleMethod(nameof(GetDefault), true);

    /// <summary>Creates default(T) expression for provided <paramref name="type"/>.</summary>
    public static Expression GetDefaultValueExpression(this Type type) =>
        !type.IsValueType() ? Constant(null, type) : (Expression)Call(GetDefaultMethod.MakeGenericMethod(type), Empty<Expression>());
  }

  /// <summary>Provides pretty printing/debug view for number of types.</summary>
  public static class PrintTools
  {
    /// <summary>Default separator used for printing enumerable.</summary>
    public static string DefaultItemSeparator = ", " + NewLine;

    /// <summary>Prints input object by using corresponding Print methods for know types.</summary>
    /// <param name="s">Builder to append output to.</param> <param name="x">Object to print.</param>
    /// <param name="quote">(optional) Quote to use for quoting string object.</param>
    /// <param name="itemSeparator">(optional) Separator for enumerable.</param>
    /// <param name="getTypeName">(optional) Custom type printing policy.</param>
    /// <returns>String builder with appended output.</returns>
    public static StringBuilder Print(this StringBuilder s, object x,
        string quote = "\"", string itemSeparator = null, Func<Type, string> getTypeName = null) =>
        x == null ? s.Append("null")
        : x is string ? s.Print((string)x, quote)
        : x is Type ? s.Print((Type)x, getTypeName)
        : x is IPrintable ? ((IPrintable)x).Print(s, (b, p) => b.Print(p, quote, itemSeparator, getTypeName))
        : x is IScope || x is Request ? s.Append(x) // prevent recursion for IEnumerable
        : x.GetType().IsEnum() ? s.Print(x.GetType()).Append('.').Append(Enum.GetName(x.GetType(), x))
        : (x is IEnumerable<Type> || x is IEnumerable) &&
            !x.GetType().IsAssignableTo(typeof(IEnumerable<>).MakeGenericType(x.GetType())) // exclude infinite recursion and StackOverflowEx
            ? s.Print((IEnumerable)x, itemSeparator ?? DefaultItemSeparator, (_, o) => _.Print(o, quote, null, getTypeName))
        : s.Append(x);

    /// <summary>Appends string to string builder quoting with <paramref name="quote"/> if provided.</summary>
    /// <param name="s">String builder to append string to.</param> <param name="str">String to print.</param>
    /// <param name="quote">(optional) Quote to add before and after string.</param>
    /// <returns>String builder with appended string.</returns>
    public static StringBuilder Print(this StringBuilder s, string str, string quote = "\"") =>
        quote == null ? s.Append(str) : s.Append(quote).Append(str).Append(quote);

    /// <summary>Prints enumerable by using corresponding Print method for known item type.</summary>
    /// <param name="s">String builder to append output to.</param>
    /// <param name="items">Items to print.</param>
    /// <param name="separator">(optional) Custom separator if provided.</param>
    /// <param name="printItem">(optional) Custom item printer if provided.</param>
    /// <returns>String builder with appended output.</returns>
    public static StringBuilder Print(this StringBuilder s, IEnumerable items,
        string separator = ", ", Action<StringBuilder, object> printItem = null)
    {
      if (items == null)
        return s;
      printItem = printItem ?? ((_, x) => _.Print(x));
      var itemCount = 0;
      foreach (var item in items)
        printItem(itemCount++ == 0 ? s : s.Append(separator), item);
      return s;
    }

    /// <summary>Default delegate to print Type details: by default prints Type FullName and
    /// skips namespace if it start with "System."</summary>
    public static Func<Type, string> GetTypeNameDefault = t =>
#if DEBUG
            t.Name;
#else
            t.FullName != null && t.Namespace != null && !t.Namespace.StartsWith("System") ? t.FullName : t.Name;
#endif

    // todo: @bug Use the version from the FEC v3
    /// <summary>Pretty prints the <paramref name="type"/> in proper C# representation.
    /// <paramref name="getTypeName"/>Allows to specify if you want Name instead of FullName.</summary>
    public static StringBuilder Print(this StringBuilder s, Type type, Func<Type, string> getTypeName = null)
    {
      if (type == null)
        return s;

      var isArray = type.IsArray;
      if (isArray)
        type = type.GetElementType();

      var typeName = (getTypeName ?? GetTypeNameDefault).Invoke(type);

      if (!type.IsGeneric())
        return s.Append(typeName.Replace('+', '.'));

      var tickIndex = typeName.IndexOf('`');
      if (tickIndex != -1)
        typeName = typeName.Substring(0, tickIndex);

      s.Append(typeName.Replace('+', '.'));

      s.Append('<');
      var genericArgs = type.GetGenericParamsAndArgs();
      if (type.IsGenericDefinition())
        s.Append(',', genericArgs.Length - 1);
      else
        s.Print(genericArgs, ", ", (b, t) => b.Print((Type)t, getTypeName));
      s.Append('>');

      if (isArray)
        s.Append("[]");

      return s;
    }

    /// <summary>Pretty-prints the type</summary>
    public static string Print(this Type type, Func<Type, string> getTypeName = null) =>
        new StringBuilder().Print(type, getTypeName).ToString();
  }

  /// <summary>Ports some methods from .Net 4.0/4.5</summary>
  public static partial class Portable
  {
    /// <summary>Portable version of Assembly.GetTypes or Assembly.DefinedTypes.</summary>
    public static IEnumerable<Type> GetAssemblyTypes(Assembly a) => _getAssemblyTypes.Value(a);

    private static readonly Lazy<Func<Assembly, IEnumerable<Type>>> _getAssemblyTypes = Lazy.Of(GetAssemblyTypesMethod);
    private static Func<Assembly, IEnumerable<Type>> GetAssemblyTypesMethod()
    {
      var asmExpr = Parameter(typeof(Assembly), "a");

      var definedTypesProperty = typeof(Assembly).GetTypeInfo().GetDeclaredProperty("DefinedTypes");
      if (definedTypesProperty != null)
      {
        if (definedTypesProperty.PropertyType == typeof(IEnumerable<TypeInfo>))
          return a => ((IEnumerable<TypeInfo>)definedTypesProperty.GetValue(a, null)).Select(t => t.AsType());
        return a => (IEnumerable<Type>)definedTypesProperty.GetValue(a, null);
      }

      var getTypesMethod = typeof(Assembly).GetTypeInfo().GetDeclaredMethod("GetTypes");
      return a => (IEnumerable<Type>)getTypesMethod.Invoke(a, Empty<object>());
    }

    /// <summary>Portable version of PropertyInfo.GetGetMethod.</summary>
    public static MethodInfo GetGetMethodOrNull(this PropertyInfo p, bool includeNonPublic = false)
    {
      var name = "get_" + p.Name;
      var methods = p.DeclaringType.GetTypeInfo().DeclaredMethods.ToArrayOrSelf();
      for (var i = 0; i < methods.Length; i++)
      {
        var m = methods[i];
        if (m.IsSpecialName && (includeNonPublic || m.IsPublic) && m.Name == name)
          return m;
      }

      return null;
    }

    /// <summary>Portable version of PropertyInfo.GetSetMethod.</summary>
    public static MethodInfo GetSetMethodOrNull(this PropertyInfo p, bool includeNonPublic = false)
    {
      var name = "set_" + p.Name;
      var methods = p.DeclaringType.GetTypeInfo().DeclaredMethods.ToArrayOrSelf();
      for (var i = 0; i < methods.Length; i++)
      {
        var m = methods[i];
        if (m.IsSpecialName && (includeNonPublic || m.IsPublic) && m.Name == name)
          return m;
      }

      return null;
    }

    private static readonly Lazy<Func<int>> _getEnvCurrentManagedThreadId = Lazy.Of<Func<int>>(() =>
    {
      var method = typeof(Environment).GetMethodOrNull("get_CurrentManagedThreadId", Empty<Type>());
      if (method == null)
        return null;
      return () => (int)method.Invoke(null, (Empty<object>()));
    });

    /// <summary>Returns managed Thread ID either from Environment or Thread.CurrentThread whichever is available.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static int GetCurrentManagedThreadID()
    {
      var resultID = -1;
      GetCurrentManagedThreadID(ref resultID);
      if (resultID == -1)
        resultID = _getEnvCurrentManagedThreadId.Value();
      return resultID;
    }

    static partial void GetCurrentManagedThreadID(ref int threadID);
  }
} // end of DryIoc namespace

#if SUPPORTS_ASYNC_LOCAL
namespace Crystal
{
	using System.Threading;

	/// <summary>Stores scopes propagating through async-await boundaries.</summary>
	public sealed class AsyncExecutionFlowScopeContext : IScopeContext
  {
    /// <summary>Statically known name of root scope in this context.</summary>
    public static readonly string ScopeContextName = typeof(AsyncExecutionFlowScopeContext).FullName;

    /// It is fine to use a default instance, cause the async local scope are actually a static one
    public static readonly AsyncExecutionFlowScopeContext Default = new AsyncExecutionFlowScopeContext();

    private static readonly AsyncLocal<IScope> _ambientScope = new AsyncLocal<IScope>();

    /// <summary>Returns current scope or null if no ambient scope available at the moment.</summary>
    /// <returns>Current scope or null.</returns>
    public IScope GetCurrentOrDefault() => _ambientScope.Value;

    /// <summary>Changes current scope using provided delegate. Delegate receives current scope as input and  should return new current scope.</summary>
    /// <param name="changeCurrentScope">Delegate to change the scope.</param>
    /// <remarks>Important: <paramref name="changeCurrentScope"/> may be called multiple times in concurrent environment.
    /// Make it predictable by removing any side effects.</remarks>
    /// <returns>New current scope. It is convenient to use method in "using (var newScope = ctx.SetCurrent(...))".</returns>
    public IScope SetCurrent(SetCurrentScopeHandler changeCurrentScope) =>
        _ambientScope.Value = changeCurrentScope(GetCurrentOrDefault());

    /// <summary>Nothing to dispose.</summary>
    public void Dispose() { }
  }
}
#endif

#if SUPPORTS_VARIANCE
namespace Crystal
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;

	/// Base type for messages
	public interface IMessage<out TResponse> { }

  /// Type for an empty response
  public struct EmptyResponse
  {
    /// Single value of empty response
    public static readonly EmptyResponse Value = new EmptyResponse();

    /// Single completed task for the empty response
    public static readonly Task<EmptyResponse> Task = System.Threading.Tasks.Task.FromResult(Value);
  }

  /// Message extensions
  public static class MessageExtensions
  {
    /// Converts the task to empty response task
    public static async Task<EmptyResponse> ToEmptyResponse(this Task task)
    {
      await task;
      return EmptyResponse.Value;
    }
  }

  /// Message with empty response
  public interface IMessage : IMessage<EmptyResponse> { }

  /// Base message handler
  public interface IMessageHandler<in M, R> where M : IMessage<R>
  {
    /// Generic handler
    Task<R> Handle(M message, CancellationToken cancellationToken);
  }

  /// Base message handler for message with empty response
  public interface IMessageHandler<in M> : IMessageHandler<M, EmptyResponse> where M : IMessage<EmptyResponse> { }

  /// Message handler middleware to handle the message and pass the result to the next middleware
  public interface IMessageMiddleware<in M, R>
  {
    /// <summary>`0` means the default registration order,
    /// lesser numbers incuding the `-1`, `-2` mean execute as a first,
    /// bigger numbers mean execute as a last</summary>
    int RelativeOrder { get; }

    /// <summary>Handles message and passes to the next middleware</summary>
    Task<R> Handle(M message, CancellationToken cancellationToken, Func<Task<R>> nextMiddleware);
  }

  /// Base class for implementing async handlers
  public abstract class AsyncMessageHandler<M, R> : IMessageHandler<M, R>
      where M : IMessage<R>
  {
    /// Base method to implement in your inheritor
    protected abstract Task<R> Handle(M message, CancellationToken cancellationToken);

    async Task<R> IMessageHandler<M, R>.Handle(M message, CancellationToken cancellationToken) =>
        await Handle(message, cancellationToken).ConfigureAwait(false);
  }

  /// Sequential middleware type of message handler decorator
  public class MiddlewareMessageHandler<M, R> : IMessageHandler<M, R> where M : IMessage<R>
  {
    private readonly IMessageHandler<M, R> _handler;
    private readonly IEnumerable<IMessageMiddleware<M, R>> _middlewares;

    /// Decorates message handler with optional middlewares
    public MiddlewareMessageHandler(IMessageHandler<M, R> handler, IEnumerable<IMessageMiddleware<M, R>> middlewares)
    {
      _handler = handler;
      _middlewares = middlewares;
    }

    /// Composes middlewares with handler
    public Task<R> Handle(M message, CancellationToken cancellationToken)
    {
      return _middlewares
          .OrderBy(x => x.RelativeOrder)
          .Reverse()
          .Aggregate(
              new Func<Task<R>>(() => _handler.Handle(message, cancellationToken)),
              (f, middleware) => () => middleware.Handle(message, cancellationToken, f))
          .Invoke();
    }
  }

  /// Broadcasting type of message handler decorator
  public class BroadcastMessageHandler<M> : IMessageHandler<M, EmptyResponse>
      where M : IMessage<EmptyResponse>
  {
    private readonly IEnumerable<IMessageHandler<M, EmptyResponse>> _handlers;

    /// Constructs the hub with the handler and optional middlewares
    public BroadcastMessageHandler(IEnumerable<IMessageHandler<M, EmptyResponse>> handlers) =>
        _handlers = handlers;

    /// Composes middlewares with handler
    public async Task<EmptyResponse> Handle(M message, CancellationToken cancellationToken)
    {
      await Task.WhenAll(_handlers.Select(h => h.Handle(message, cancellationToken)));
      return EmptyResponse.Value;
    }
  }

  // todo: @feature add SendToAll with the reducing the results
  // todo: @feature add NotifyAll without waiting for results
  /// <summary>The central mediator entry-point</summary>
  public class MessageMediator
  {
    private readonly IResolver _resolver;

    /// <summary>Constructs the mediator</summary>
    public MessageMediator(IResolver resolver) =>
        _resolver = resolver;

    /// <summary>Sends the message with response to the resolved Single handler</summary>
    public Task<R> Send<M, R>(M message, CancellationToken cancellationToken) where M : IMessage<R> =>
        _resolver.Resolve<IMessageHandler<M, R>>().Handle(message, cancellationToken);

    /// <summary>Sends the message with empty response to resolved Single handler</summary>
    public Task Send<M>(M message, CancellationToken cancellationToken) where M : IMessage<EmptyResponse> =>
        _resolver.Resolve<IMessageHandler<M, EmptyResponse>>().Handle(message, cancellationToken);
  }
}
#endif

namespace Crystal
{
	/// <summary>The testing utility</summary>
	public interface ITest
  {
    /// <summary>Runs the tests and should return the number of run tests</summary>
    int Run();
  }
}