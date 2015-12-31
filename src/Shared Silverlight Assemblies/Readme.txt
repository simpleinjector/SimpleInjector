This file explains the expected semantics IServiceLocator implementations must implement to properly conform to this interface, and a few implementation notes.

==== Specification ====

**** GetInstance(Type, string) ****

This is the core method for retrieving a single instance from the container.

This method MUST NOT return null. It MUST return either an instance that implements the requested type or throw an ActivationException.
No other exception type is allowed (except for the usual CLR rules for things like ThreadAbortException).

The implementation should be designed to expect a null for the string key parameter, and MUST interpret this as a request to get the "default" instance for the requested type. The meaning of "default" depends on the underlying container and how it is configured. A string of length 0 is considered to be different from a null, and implementers are free to choose what a string of length 0 as a key means.

**** GetAllInstances(Type) ****

This is the core method for retrieving multiple instances from the container.

If the container contains no instances of the requested type, this method MUST return an enumerator of length 0 instead of throwing an exception.

If an exception occurs while activating instances during enumeration, this method SHOULD throw an ActivationException and abort the enumeration. However, it may also choose to simply skip that object and continue enumerating.

**** Overload Behavior ****

A call to:

    object IServiceLocator.GetInstance(serviceType)
    
MUST be exactly equivalent to a call to:

    object IServiceLocator.GetInstance(serviceType, null)
    
A call to:

    TService IServiceLocator.GetInstance<TService>()

MUST be exactly equivalent to a call to:

    (TService)IServiceLocator.GetInstance(typeof(TService), null)
    
A call to:

    TService IServiceLocator.GetInstance<TService>(key)
    
MUST be exactly equivalent to a call to:

    (TService)IServiceLocator.GetInstance(typeof(TService), key)
    
A call to:

    IEnumerable<TService> IServiceLocator.GetAllInstances<TService>()
    
Must be exactly equivalent to a call to:

    IEnumerable<object> IServiceLocator.GetAllInstances(typeof(TService))
    
with the exception that the objects returned by the enumerator are already cast to type TService.

**** Throwing ActivationException ****

When throwing an ActivationException, the message string is explicitly undefined by this specification; the adapter implementors may format this message in any way they choose.

When throwing an ActivationException, the original exception MUST be returned as the value of the InnerException property.

==== ServiceLocatorImplBase ====

This class is not part of the specification; consumers should only reference the IServiceLocator interface. ServiceLocatorImplBase is provided as a convenience for implementors of IServiceLocator. It implements the correct overload semantics and exception wrapping behavior defined above. You just need to implement the two protected methods DoGetInstance and DoGetAllInstances and the rest will just work. In addition, the two protected methods FormatActivationExceptionMessage and FormatActivateAllExceptionMessage are provided if you wish to customize the error message reported in the exceptions.

==== Why is ActivationException a partial class? ====

Implementing ISerializable for exceptions is a .NET best practice for the desktop CLR. However, I anticipate a port to Silverlight for this code - many containers are already supporting Silverlight beta 2 or are about to. Silverlight does not support classic binary serialization. By making this a partial class and segregating the serialization details into a separate file, a future Silverlight port can simply leave the .Desktop.cs file out of the project and the incompatible code will be seamlessly removed.

