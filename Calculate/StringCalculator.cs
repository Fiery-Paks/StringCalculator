using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculate
{
    public class StringCalculator
    {
        private readonly IReadOnlyCollection<char> Arithmetic_operators = new List<char>() { '*', '/', '+', '-', '(', ')' };

        private bool IsConvertingToDouble(string elemet)
        {
            if (double.TryParse(elemet, out _)) return true;
            return false;
        }

        private List<string> Separation(string enterText)
        {
            string remainder = enterText.Replace(" ", "").Replace("\n", "").Replace("\t", "");
            List<string> split_list = new List<string>();
            if (String.IsNullOrEmpty(remainder)) throw new Exception("Введите данные");
            if (IsConvertingToDouble(remainder))
            {
                split_list.Add(remainder);
                return split_list;
            }
            for (int i = 0; i < enterText.Length; i++)
            {
                if (Arithmetic_operators.Contains(enterText[i]))
                {
                    split_list.Add(remainder.Split(enterText[i]).First());
                    remainder = enterText.Remove(0, i + 1);
                    split_list.Add(enterText[i].ToString());
                    if (remainder.Length != 0 && IsConvertingToDouble(remainder)) split_list.Add(remainder);
                }
            }
            return split_list;
        }

        private Types Type_definition(string element)
        {
            if (IsConvertingToDouble(element))
                return Types.Number;
            else if (Arithmetic_operators.Any(x => element == x.ToString() && element != "(" && element != ")"))
                return Types.Arithmetic_operators;
            else if (element == "(" || element == ")")
                return Types.Brace;
            return Types.Trash;
        }

        private List<ElementInfo> List_Edit(List<string> split_list)
        {
            List<ElementInfo> elements = new List<ElementInfo>();
            for (int i = 0; i < split_list.Count; i++)
                elements.Add(new ElementInfo(split_list[i], Type_definition(split_list[i]), i));
            elements = elements.Where(x => x.type != Types.Trash).ToList();
            CleanList(ref elements);
            SetNewIndex(ref elements);
            return elements;
        }

        private void CleanList(ref List<ElementInfo> elements)
        {
            int CountList = elements.Count;
            for (int i = 0; i < CountList; i++)
            {
                if (elements.First().content == "-" && elements[1].type == Types.Number)
                {
                    elements[0] = new ElementInfo(elements[0].content + elements[1].content, Types.Number, 0);
                    elements.RemoveAt(1);
                }
                else if (elements.First().type == Types.Arithmetic_operators)
                    elements.Remove(elements.First());
                else if (elements.Last().type == Types.Arithmetic_operators)
                    elements.Remove(elements.Last());
                else if (elements.First().type != Types.Arithmetic_operators && elements.Last().type != Types.Arithmetic_operators) break;
            }
            for (int i = 0, Cbrace = 0; i < elements.Count; i++)
            {
                if (elements[i].type == Types.Arithmetic_operators && elements[i + 1].type == Types.Arithmetic_operators)
                    throw new Exception("Ошибка");
                if (elements[i].content == "(") Cbrace++;
                else if (elements[i].content == ")") Cbrace--;
                if (i == elements.Count - 1 && Cbrace != 0) throw new Exception("Ошибка");
            }
        }

        private double BraceSearch(List<string> split_list)
        {
            List<ElementInfo> elements = List_Edit(split_list);
            bool find = FindRangeBrace(elements, out IntPair intPair);
            if (find == true)
            {
                List<ElementInfo> part = elements.GetRange(intPair.istart, intPair.count);
                SetNewIndex(ref part);
                ElementInfo elementinfo = new ElementInfo(BraceSearch(part).ToString(), Types.Number, intPair.istart - 1);
                elements[intPair.istart - 1] = elementinfo;
                elements.RemoveRange(intPair.istart, intPair.count + 1);
                SetNewIndex(ref elements);
                return BraceSearch(elements);
            }
            else
                return CourseOfAction(elements);
        }
        private double BraceSearch(List<ElementInfo> elements)
        {
            bool find = FindRangeBrace(elements, out IntPair intPair);
            if (find == true)
            {
                List<ElementInfo> part = elements.GetRange(intPair.istart, intPair.count);
                SetNewIndex(ref part);
                ElementInfo elementinfo = new ElementInfo(BraceSearch(part).ToString(), Types.Number, intPair.istart - 1);
                elements[intPair.istart - 1] = elementinfo;
                elements.RemoveRange(intPair.istart, intPair.count + 1);
                SetNewIndex(ref elements);
                return BraceSearch(elements);
            }
            else
                return CourseOfAction(elements);
        }

        private bool FindRangeBrace(List<ElementInfo> elements, out IntPair intPair)
        {
            intPair = new IntPair();
            if (!elements.Any(x => x.type == Types.Brace)) return false;
            for (int i = 0, brace = 0; i < elements.Count; i++)
            {
                if (elements[i].content == "(")
                {
                    if (brace == 0)
                        intPair.istart = i + 1;
                    brace++;
                }
                else if (elements[i].content == ")")
                {
                    brace--;
                    if (brace == 0)
                    {
                        intPair.count = i - intPair.istart;
                        break;
                    }
                }
            }
            return true;
        }

        private double CourseOfAction(List<ElementInfo> elements)
        {
            int _Count = elements.Where(x => x.type == Types.Arithmetic_operators).ToList().Count;
            for (int i = 0; i < _Count; i++)
                CalcuationElement(ref elements);
            return Convert.ToDouble(elements.First().content);
        }

        private void CalcuationElement(ref List<ElementInfo> elements)
        {
            int index = FindFirstAct(elements);
            elements[index] = new ElementInfo(Calcutating(Convert.ToDouble(elements[index - 1].content), Convert.ToDouble(elements[index + 1].content), elements[index].content).ToString(), Types.Number, index);
            elements.RemoveAt(index + 1);
            elements.RemoveAt(index - 1);
            SetNewIndex(ref elements);
        }

        private void SetNewIndex(ref List<ElementInfo> elements)
        {
            for (int i = 0; i < elements.Count; i++)
                elements[i] = new ElementInfo(elements[i].content, elements[i].type, i);
        }

        private int FindFirstAct(List<ElementInfo> elements)
        {
            if (elements.Any(x => x.content == "*") || elements.Any(x => x.content == "/"))
                return (elements.Where(x => x.content == "*").DefaultIfEmpty(new ElementInfo("", new Types(), int.MaxValue)).First().indexn < elements.Where(x => x.content == "/").DefaultIfEmpty(new ElementInfo("", new Types(), int.MaxValue)).First().indexn) ?
                        elements.FirstOrDefault(x => x.content == "*").indexn :
                        elements.FirstOrDefault(x => x.content == "/").indexn;
            if (elements.Any(x => x.content == "+") || elements.Any(x => x.content == "-"))
                return (elements.Where(x => x.content == "+").DefaultIfEmpty(new ElementInfo("", new Types(), int.MaxValue)).First().indexn < elements.Where(x => x.content == "-").DefaultIfEmpty(new ElementInfo("", new Types(), int.MaxValue)).First().indexn) ?
                        elements.FirstOrDefault(x => x.content == "+").indexn :
                        elements.FirstOrDefault(x => x.content == "-").indexn;
            return -1;
        }

        private double Calcutating(double number1, double number2, string operation)
        {
            switch (operation)
            {
                case "+": return number1 + number2;
                case "-": return number1 - number2;
                case "/": return number2 != 0 ? number1 / number2 : throw new Exception("Запрещенно делить на 0");
                case "*": return number1 * number2;
                default: return 0;
            }
        }

        public double Converting(string enterText)
        {
            return BraceSearch(Separation(enterText));
        }

        private struct IntPair
        {
            internal int istart;
            internal int count;

            internal IntPair(int istart, int count)
            {
                this.istart = istart;
                this.count = count;
            }
        }

        private struct ElementInfo
        {
            internal string content;
            internal Types type;
            internal int indexn;

            internal ElementInfo(string elem, Types typ)
            {
                content = elem;
                type = typ;
                indexn = -1;
            }

            internal ElementInfo(string elem, int num)
            {
                content = elem;
                type = new Types();
                indexn = num;
            }

            internal ElementInfo(string elem, Types typ, int num)
            {
                content = elem;
                type = typ;
                indexn = num;
            }
        }

        private enum Types
        {
            Arithmetic_operators,
            Brace,
            Number,
            Trash
        }
    }
}

