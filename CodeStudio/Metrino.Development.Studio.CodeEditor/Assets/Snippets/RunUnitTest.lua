local file_path = "path_to_a_file"
local test_result = RunUnitTest("UnitTestOlm.Access.TestAccessAnalysis", "AccessEndOfFiberTest", file_path)
if not test_result.Pass then
    print("  * " .. test_result.Error)
end
