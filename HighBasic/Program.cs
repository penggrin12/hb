namespace HighBasic;

public static class Program
{
    static void Main()
    {
        const string source = """
: this is a comment
noop : comments can also be inline

var message "hello there ms biscuits 🩷 (this is a string literal assigned to a variable)"
println $message : reading a variable value (by prefixing with $)

println "this is a string literal (not using a variable)"

print "this is also a string literal but without a"
println " new line"

vars cool_vars:this is a comment too
println $cool_vars
""";

        Runtime runtime = new Runtime()
            .InsertStandardLibrary();

        runtime.DoString(source);
    }
}
