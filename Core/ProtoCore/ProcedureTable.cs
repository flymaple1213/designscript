﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ProtoCore.DSASM
{
    public enum ProcedureDistance
    {
        // TODO Jun: Corecion score may have to be expanded depending on specific types

        kCoerceScore = 10,
        kCoerceToPointer = 20,
        kCoerceDoubleToIntScore = 5,
        kCoerceIntToDoubleScore = 30,
        kCoerceBaseClass = 40,
        kExactMatchScore = 50,
        kNotMatchScore = 0,
        kExactMatchDistance = 0,
        kMaxDistance = Int32.MaxValue,
        kHasOmittedDefaultArgDistance = 5, //[TODO] Fuqiang: check with Jun about the value, should it be larger than kCoerceScore?
        kInvalidDistance = -1
    
    }

    public struct ArgumentInfo
    {
        public string Name { get; set; }
        public bool isDefault;
        public ProtoCore.AST.Node defaultExpression;
    }

    public class ProcedureNode
    {
        public string name;                         // Name of the procedure
        public int pc;                              // First instruction of the procedure
        public int localCount;                      // Number of local variables
        public List<ProtoCore.Type> argTypeList;    // List of arguments required by the procedure
        public List<ArgumentInfo> argInfoList;      // List of arguments' information (default value)
        public int? firstLocal;                     // Symbol index of the first local variable
        public ProtoCore.Type returntype;           // Procedure return data type
        public bool isConstructor;                  // Flag whether procedure is a constructor or not
        public bool isStatic;                       // Flag whether procedure is a static function or not
        public int runtimeIndex { get; set; }       // Index of the procedure at the runtime executable tables
        public int procId { get; set; }             // Index of the procedure in its procedure table
        public int classScope { get; set; }         // Index of the class that the procedure belongs to
        public AccessSpecifier access {get; set;}
        public bool isThisCallReplication { get; set; }
		public List<AttributeEntry> Attributes { get; set; }		
        public bool isExternal { get; set; }
        public bool isAssocOperator { get; set; }
        public bool isAutoGenerated { get; set; }
        public bool isAutoGeneratedThisProc { get; set; }
        public ProtoFFI.FFIMethodAttributes MethodAttribute { get; set; }

        public Stack<ProtoCore.AssociativeGraph.UpdateNodeRef> updatedGlobals { get; set; }
        public Stack<ProtoCore.AssociativeGraph.UpdateNodeRef> updatedProperties { get; set; }
        public List<ProtoCore.AssociativeGraph.UpdateNodeRef> updatedArguments { get; set; }
        public Dictionary<string, List<ProtoCore.AssociativeGraph.UpdateNodeRef>> updatedArgumentProperties { get; set; }
        public Dictionary<string, List<ProtoCore.AssociativeGraph.UpdateNode>> updatedArgumentArrays { get; set; }

        public ProcedureNode()
        {
            procId = DSASM.Constants.kInvalidIndex;
            classScope = DSASM.Constants.kInvalidIndex;
            argTypeList = new List<Type>();
            argInfoList = new List<ArgumentInfo>();
            isConstructor = false;
            isStatic = false;
            firstLocal = null;
            access = AccessSpecifier.kPublic;
            isThisCallReplication = false;
            isAutoGenerated = false;
            isAutoGeneratedThisProc = false;

            updatedGlobals = new Stack<AssociativeGraph.UpdateNodeRef>();
            updatedProperties = new Stack<AssociativeGraph.UpdateNodeRef>();
            updatedArguments = new List<AssociativeGraph.UpdateNodeRef>();
            updatedArgumentProperties = new Dictionary<string, List<AssociativeGraph.UpdateNodeRef>>();
            updatedArgumentArrays = new Dictionary<string, List<AssociativeGraph.UpdateNode>>();
        }

        public bool IsEqual(ProcedureNode rhs)
        {
            return procId == rhs.procId && classScope == rhs.classScope && localCount == rhs.localCount && name == rhs.name;
        }
                
                /*
                // Check the expected type 
                switch (rcvdType)
                {
                    case (int)PrimitiveType.kTypeVar:
                        {
                            score = (int)ProcedureDistance.kCoerceScore;
                            break;
                        }
                    case (int)PrimitiveType.kTypeDouble:
                        {
                            score = (int)ProcedureDistance.kCoerceScore;
                            break;
                        }
                    case (int)PrimitiveType.kTypeInt:
                        {
                            score = (int)ProcedureDistance.kCoerceIntToDoubleScore;
                            break;
                        }
                    case (int)PrimitiveType.kTypeNull:
                        {
                            score = (int)ProcedureDistance.kCoerceScore;
                            break;
                        }
                    case (int)PrimitiveType.kTypeArray:
                        {
                            score = (int)ProcedureDistance.kCoerceScore;
                            break;
                        }
                    default:
                        {
                            // Procedure expects a user-defined type
                            break;
                        }
                }
                 * */
            
       
        public int GetDistance(string name, List<ProtoCore.Type> args, ProtoCore.DSASM.ClassTable classtable)
        {
            Debug.Assert(null != classtable);
            int defaultArgNum = argInfoList.Count(X => X.isDefault);
            Debug.Assert(argTypeList.Count - args.Count <= defaultArgNum);

            int distance = (int)ProcedureDistance.kMaxDistance;

            if (0 == args.Count)
            {
                distance = (int)ProcedureDistance.kExactMatchDistance;
            }
            else
            {
                isThisCallReplication = false;
                // Check if all the types match the current function at 'n'
                for (int i = 0; i < args.Count; ++i)
                {
                    int rcvdType = args[i].UID;
                    int expectedType = argTypeList[i].UID;
                    int currentScore = (int)ProcedureDistance.kNotMatchScore;
                                        
                    //check whether it is replication call
                    if (argTypeList[i].rank != -1 && args[i].rank > argTypeList[i].rank)
                    {
                        isThisCallReplication = true;
                    }

                    //Fuqiang: For now disable rank check
                    //if function is expecting array, but non-array or array of lower rank is passed, break.
                    //if (args[i].rank != -1 && args[i].UID != (int)PrimitiveType.kTypeVar && args[i].rank < argTypeList[i].rank)

                    //Only enable type check, and array and non-array check
                    if (args[i].rank != -1 && args[i].UID != (int)PrimitiveType.kTypeVar && !args[i].IsIndexable && argTypeList[i].IsIndexable)
                    {
                        distance = (int)ProcedureDistance.kMaxDistance;
                        break;
                    }
                    else if (expectedType == rcvdType)
                    {
                        currentScore = (int)ProcedureDistance.kExactMatchScore;
                    }
                    else
                    {
                        currentScore = classtable.ClassNodes[rcvdType].GetCoercionScore(expectedType);
                        if (currentScore == (int)ProcedureDistance.kNotMatchScore)
                        {
                            distance = (int)ProcedureDistance.kMaxDistance;
                            break;
                        }
                    }
                    distance -= currentScore;
                }
            }
            return distance;
        }
    }

    public class ProcedureTable
    {
        public int runtimeIndex { get; private set; }
        public List<ProcedureNode> procList { get; set; }
        
        public ProcedureTable(int runtimeindex)
        {
            runtimeIndex = runtimeindex;
            procList = new List<ProcedureNode>();
        }

        public int Append(ProcedureNode node)
        {
            if (ProtoCore.DSASM.Constants.kInvalidIndex == IndexOfExact(node.name, node.argTypeList))
            {
                procList.Add(node);
                node.procId = procList.Count - 1;
                return node.procId;
            }
            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        
        public ProcedureNode GetFirst(string name)
        {
            ProcedureNode procReturn = null;
            for (int n = 0; n < procList.Count; ++n)
            {
                if (name == procList[n].name)
                {
                    procReturn = procList[n];
                    break;
                }
            }
            return procReturn;
        }

        public ProcedureNode GetFirst(string name, int argCount)
        {
            ProcedureNode procReturn = null;
            for (int n = 0; n < procList.Count; ++n)
            {
                if (name == procList[n].name && argCount == procList[n].argTypeList.Count)
                {
                    procReturn = procList[n];
                    break;
                }
            }
            return procReturn;
        }

        public ProcedureNode GetFirstStatic(string name)
        {
            ProcedureNode procReturn = null;
            for (int n = 0; n < procList.Count; ++n)
            {
                if (name == procList[n].name && procList[n].isStatic)
                {
                    procReturn = procList[n];
                    break;
                }
            }
            return procReturn;
        }


#if NO_DISTANCE_CHECK

        public int IndexOf(string name, List<int> argTypeList)
        {
            for (int n = 0; n < procList.Count; ++n)
            {
                if (name == procList[n].name && argTypeList.Count == procList[n].argTypeList.Count)
                {
                    // TODO Jun: Move this into a List<> comparison utility function
                    bool matchAllTypes = true;
                    
                    // Check if all the types match the current function at 'n'
                    for (int i = 0; i < procList[n].argTypeList.Count; ++i)
                    {
                        bool untypedArg = (int)PrimitiveType.kTypeVar == procList[n].argTypeList[i];
                        // TODO Jun: Allows array types until the proper design of method resolution
                        bool allowType = (int)PrimitiveType.kTypeArray == argTypeList[i];

                        // TODO Jun: Allow int passed to a double for the interim
                        bool allowIntToDouble = (int)PrimitiveType.kTypeDouble == procList[n].argTypeList[i] && (int)PrimitiveType.kTypeInt == argTypeList[i];

                        // TODO Jun: Allow null passed for the interim
                        bool allowNull = (int)PrimitiveType.kTypeNull == argTypeList[i];

                        //Allow var
                        bool allowVar = (int)PrimitiveType.kTypeVar == argTypeList[i];

                        
                        if (!untypedArg && !allowNull && !allowVar && !allowIntToDouble && !allowType && argTypeList[i] != procList[n].argTypeList[i])                            
                        {
                            matchAllTypes = false;
                            break;
                        }
                    }

                    if (matchAllTypes)
                    {
                        return n;
                    }
                }
            }
            return (int)ProtoCore.DSASM.Constants.kInvalidIndex;
        }
#else
        public int IndexOf(string name, List<ProtoCore.Type> argTypeList, ProtoCore.DSASM.ClassTable classtable, bool isStaticOrConstructor = false)
        {
#if STATIC_TYPE_CHECKING
            int distance = (int)ProcedureDistance.kMaxDistance;
#endif
            int currentProcedure = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool functionPointerCheck = argTypeList == null;

            if (functionPointerCheck) // check for function pointer
            {
                for (int n = 0; n < procList.Count; ++n)
                {
                    if (name == procList[n].name)
                    {
                        currentProcedure = n;
                        break;
                    }
                }
                return currentProcedure;
            }

            // For every procedure in this table
            for (int n = 0; n < procList.Count; ++n)
            {
                int defaultArgNum = procList[n].argInfoList.Count(X => X.isDefault);
                if (name == procList[n].name
                    && procList[n].argTypeList.Count >= argTypeList.Count
                    && (procList[n].argTypeList.Count - argTypeList.Count <= defaultArgNum))
                {
#if STATIC_TYPE_CHECKING
                    int currentDist = procList[n].GetDistance(name, argTypeList, classtable);
                    if (currentDist < distance && procList[n].argTypeList.Count > argTypeList.Count)
                        //Fuqiang: to differentiate a function with all the arguments provided and a function with default arguments not provided.
                        currentDist += (int)ProcedureDistance.kHasOmittedDefaultArgDistance;    
                    if (currentDist < distance)
                    {
                        distance = currentDist;
                        currentProcedure = n;
                        if ((int)ProcedureDistance.kExactMatchDistance == distance)
                        {
                            // The procedure is an exact match
                            // There is nothing more to check
                            break;
                        }
                    }
#else
                    if (!isStaticOrConstructor ||
                        (isStaticOrConstructor && (procList[n].isStatic || procList[n].isConstructor)))
                    {
                        currentProcedure = n;
                        break;
                    }
#endif
                }
            }
            return currentProcedure;
        }

#endif

        public int IndexOfExact(string name, List<ProtoCore.Type> args)
        {
            // This functions attempts to find an exact match and return its index
            // Iterate through all defined functions
            for (int n = 0; n < procList.Count; ++n)
            {
                if (name == procList[n].name && args.Count == procList[n].argTypeList.Count)
                {
                    bool isMatch = true;
                    for (int i = 0; i < args.Count; ++i)
                    {
                        if ((procList[n].argTypeList[i].UID != args[i].UID) ||
                            (procList[n].argTypeList[i].IsIndexable != args[i].IsIndexable) ||
                            (procList[n].argTypeList[i].rank != args[i].rank))
                        {
                            isMatch = false;
                            break;
                        }
                    }

                    if (isMatch)
                    {
                        return n;
                    }
                }
            }
            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        public int IndexOfFirst(string name)
        {
            for (int n = 0; n < procList.Count; ++n)
            {
                if (name == procList[n].name)
                {
                    return n;
                }
            }
            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }
    }
}
