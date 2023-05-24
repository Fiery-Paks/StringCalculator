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
            string remainder = enterText.Replace(" ", "");
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
            elements = elements.Where(x => x._type != Types.Trash).ToList();
            CleanList(ref elements);
            SetNewIndex(ref elements);
            return elements;
        }

        private void CleanList(ref List<ElementInfo> elements)
        {
            int CountList = elements.Count;
            for (int i = 0; i < CountList; i++)
            {
                if (elements.First()._element == "-" && elements[1]._type == Types.Number)
                {
                    elements[0] = new ElementInfo(elements[0]._element + elements[1]._element, Types.Number, 0);
                    elements.RemoveAt(1);
                }
                else if (elements.First()._type == Types.Arithmetic_operators)
                    elements.Remove(elements.First());
                else if (elements.Last()._type == Types.Arithmetic_operators)
                    elements.Remove(elements.Last());
                else if (elements.First()._type != Types.Arithmetic_operators && elements.Last()._type != Types.Arithmetic_operators) break;
            }
            for (int i = 0, Cbrace = 0; i < elements.Count; i++)
            {
                if (elements[i]._type == Types.Arithmetic_operators && elements[i + 1]._type == Types.Arithmetic_operators)
                    throw new Exception("Ошибка");
                if (elements[i]._element == "(") Cbrace++;
                else if (elements[i]._element == ")") Cbrace--;
                if (i == elements.Count - 1 && Cbrace != 0) throw new Exception("Ошибка");
            }
        }

        private double BraceSearch(List<string> split_list)
        {
            List<ElementInfo> elements = List_Edit(split_list);
            bool find = FindRangeBrace(elements, out List<int> rangebrace);

            return 0;//CourseOfAction1(elements);
        }

        private bool FindRangeBrace(List<ElementInfo> elements, out List<int> rangebrace)
        {
            rangebrace = new List<int>();
            if (!elements.Any(x => x._type == Types.Brace)) return false;
            for (int i = 0, Cbrace = 0; i < elements.Count; i++)
            {
                if (elements[i]._element == "(")
                {
                    Cbrace++;
                }
                if (elements[i]._element == ")") Cbrace--;
            }

            return true;
        }

        private double CourseOfAction1(List<ElementInfo> elements)
        {
            int _Count = elements.Where(x => x._type == Types.Arithmetic_operators).ToList().Count;
            for (int i = 0; i < _Count; i++)
                CalcuationElement(ref elements);
            return Convert.ToDouble(elements.First()._element);
        }

        private double CourseOfAction(List<string> split_list)
        {
            List<ElementInfo> elements = List_Edit(split_list);
            int _Count = elements.Where(x => x._type == Types.Arithmetic_operators).ToList().Count;
            for (int i = 0; i < _Count; i++)
                CalcuationElement(ref elements);
            return Convert.ToDouble(elements.First()._element);
        }

        private void CalcuationElement(ref List<ElementInfo> elements)
        {
            int index = FindFirstAct(elements);
            elements[index] = new ElementInfo(Calcutating(Convert.ToDouble(elements[index - 1]._element), Convert.ToDouble(elements[index + 1]._element), elements[index]._element).ToString(), Types.Number, index);
            elements.RemoveAt(index + 1);
            elements.RemoveAt(index - 1);
            SetNewIndex(ref elements);
        }

        private void SetNewIndex(ref List<ElementInfo> elements)
        {
            for (int i = 0; i < elements.Count; i++)
                elements[i] = new ElementInfo(elements[i]._element, elements[i]._type, i);
        }

        private int FindFirstAct(List<ElementInfo> elements)
        {
            if (elements.Any(x => x._element == "*") || elements.Any(x => x._element == "/"))
                return (elements.Where(x => x._element == "*").DefaultIfEmpty(new ElementInfo("", new Types(), int.MaxValue)).First()._number < elements.Where(x => x._element == "/").DefaultIfEmpty(new ElementInfo("", new Types(), int.MaxValue)).First()._number) ?
                        elements.FirstOrDefault(x => x._element == "*")._number :
                        elements.FirstOrDefault(x => x._element == "/")._number;
            if (elements.Any(x => x._element == "+") || elements.Any(x => x._element == "-"))
                return (elements.Where(x => x._element == "+").DefaultIfEmpty(new ElementInfo("", new Types(), int.MaxValue)).First()._number < elements.Where(x => x._element == "-").DefaultIfEmpty(new ElementInfo("", new Types(), int.MaxValue)).First()._number) ?
                        elements.FirstOrDefault(x => x._element == "+")._number :
                        elements.FirstOrDefault(x => x._element == "-")._number;
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
            return CourseOfAction(Separation(enterText));
        }

        private struct ElementInfo
        {
            internal string _element;
            internal Types _type;
            internal int _number;

            internal ElementInfo(string elem, Types typ)
            {
                _element = elem;
                _type = typ;
                _number = -1;
            }

            internal ElementInfo(string elem, int num)
            {
                _element = elem;
                _type = new Types();
                _number = num;
            }

            internal ElementInfo(string elem, Types typ, int num)
            {
                _element = elem;
                _type = typ;
                _number = num;
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

