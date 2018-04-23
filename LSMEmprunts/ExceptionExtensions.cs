using System;
using System.Text;

namespace LSMEmprunts
{
    public static class ExceptionExtensions
    {
        public static string CompleteDump(this Exception ex)
        {
            var sb = new StringBuilder();
            do
            {
                sb.Append(ex.Message).Append("(").Append(ex.GetType().Name).AppendLine(")");
                sb.Append("StackTrace: ").AppendLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    sb.AppendLine().AppendLine("InnerException:");
                }
                ex = ex.InnerException;
            } while (ex != null);
            return sb.ToString();
        }
    }
}
