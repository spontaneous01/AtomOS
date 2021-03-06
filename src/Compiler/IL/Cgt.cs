﻿/*
* PROJECT:          Atomix Development
* LICENSE:          Copyright (C) Atomix Development, Inc - All Rights Reserved
*                   Unauthorized copying of this file, via any medium is
*                   strictly prohibited Proprietary and confidential.
* PURPOSE:          Cgt MSIL
* PROGRAMMERS:      Aman Priyadarshi (aman.eureka@gmail.com)
*/

using System;
using System.Reflection;

using Atomix.Assembler;
using Atomix.Assembler.x86;
using Atomix.CompilerExt;
using Core = Atomix.Assembler.AssemblyHelper;

namespace Atomix.IL
{
    [ILOp(ILCode.Cgt)]
    public class Cgt : MSIL
    {
        public Cgt(Compiler Cmp)
            : base("cgt", Cmp) { }

        public override void Execute(ILOpCode instr, MethodBase aMethod)
        {
            //Cgt just check if value 2 < Value 1, if yes than push 0x1 else push 0x0
            var FirstStackItem = Core.vStack.Pop();
            var SecondStackItem = Core.vStack.Pop();

            //Here Math.Max is not necessary because they should have same size to compare, but i'm not removing this
            var xSize = Math.Max(FirstStackItem.Size, SecondStackItem.Size);
            var xCurrentLabel = ILHelper.GetLabel(aMethod, instr.Position);
            var xFinalLabel = ILHelper.GetLabel(aMethod, instr.NextPosition);

            switch (ILCompiler.CPUArchitecture)
            {
                #region _x86_
                case CPUArch.x86:
                    {
                        switch (xSize)
                        {
                            case 1:
                            case 2:
                            case 4:
                                {
                                    if (FirstStackItem.IsFloat) //If one item is float then both will
                                    {
                                        Core.AssemblerCode.Add(new Movss { DestinationReg = Registers.XMM0, SourceReg = Registers.ESP, SourceIndirect = true }); //XMM0 => Value 2
                                        Core.AssemblerCode.Add(new Assembler.x86.Add { DestinationReg = Registers.ESP, SourceRef = "0x4" });
                                        Core.AssemblerCode.Add(new Movss { DestinationReg = Registers.XMM1, SourceReg = Registers.ESP, SourceIndirect = true }); //XMM1 => Value 1
                                        Core.AssemblerCode.Add(new Cmpss { DestinationReg = Registers.XMM0, SourceReg = Registers.XMM1, PseudoCode = ComparePseudoOpcodes.LessThan });//Value 1 > Value 2
                                        Core.AssemblerCode.Add(new MovD { SourceReg = Registers.XMM0, DestinationReg = Registers.EBX });
                                        Core.AssemblerCode.Add(new And { DestinationReg = Registers.EBX, SourceRef = "0x1" });
                                        Core.AssemblerCode.Add(new Mov { SourceReg = Registers.EBX, DestinationReg = Registers.ESP, DestinationIndirect = true });
                                    }
                                    else
                                    {
                                        Core.AssemblerCode.Add(new Pop { DestinationReg = Registers.EAX }); //Value 2
                                        Core.AssemblerCode.Add(new Cmp { DestinationReg = Registers.EAX, SourceReg = Registers.ESP, SourceIndirect = true });//Value 2 - Value 1
                                        Core.AssemblerCode.Add(new Jmp { Condition = ConditionalJumpEnum.JG, DestinationRef = xCurrentLabel + ".false" });
                                        Core.AssemblerCode.Add(new Assembler.x86.Add { DestinationReg = Registers.ESP, SourceRef = "0x4" });
                                        //Not equal
                                        Core.AssemblerCode.Add(new Push { DestinationRef = "0x1" });
                                        Core.AssemblerCode.Add(new Jmp { DestinationRef = xFinalLabel });

                                        //equal
                                        Core.AssemblerCode.Add(new Label(xCurrentLabel + ".false"));
                                        Core.AssemblerCode.Add(new Assembler.x86.Add { DestinationReg = Registers.ESP, SourceRef = "0x4" });
                                        Core.AssemblerCode.Add(new Push { DestinationRef = "0x0" });
                                        Core.AssemblerCode.Add(new Jmp { DestinationRef = xFinalLabel });
                                        Core.AssemblerCode.Add(new Label(xFinalLabel));
                                    }
                                }
                                break;
                            case 8:
                                {
                                    if (FirstStackItem.IsFloat)
                                    {
                                    }
                                    else
                                    {
                                        Core.AssemblerCode.Add(new Pop { DestinationReg = Registers.EAX }); //Value 2
                                        Core.AssemblerCode.Add(new Pop { DestinationReg = Registers.EDX });

                                        Core.AssemblerCode.Add(new Pop { DestinationReg = Registers.EBX }); //Value 1
                                        Core.AssemblerCode.Add(new Pop { DestinationReg = Registers.ECX });

                                        Core.AssemblerCode.Add(new Sub { DestinationReg = Registers.EBX, SourceReg = Registers.EAX });
                                        Core.AssemblerCode.Add(new SubWithCarry { DestinationReg = Registers.ECX, SourceReg = Registers.EDX });

                                        Core.AssemblerCode.Add(new Jmp { Condition = ConditionalJumpEnum.JL, DestinationRef = xCurrentLabel + ".false" });

                                        //Less Than
                                        Core.AssemblerCode.Add(new Push { DestinationRef = "0x1" });
                                        Core.AssemblerCode.Add(new Jmp { DestinationRef = xFinalLabel });

                                        //Not Less Than
                                        Core.AssemblerCode.Add(new Label(xCurrentLabel + ".false"));
                                        Core.AssemblerCode.Add(new Push { DestinationRef = "0x0" });
                                        Core.AssemblerCode.Add(new Jmp { DestinationRef = xFinalLabel });

                                        Core.AssemblerCode.Add(new Label(xFinalLabel));
                                    }
                                }
                                break;
                        }

                    }
                    break;
                #endregion
                #region _x64_
                case CPUArch.x64:
                    {

                    }
                    break;
                #endregion
                #region _ARM_
                case CPUArch.ARM:
                    {

                    }
                    break;
                #endregion
            }
            Core.vStack.Push(4, typeof(bool));
        }
    }
}
