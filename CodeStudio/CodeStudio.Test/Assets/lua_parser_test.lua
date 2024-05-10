function exgest(file)
   local f = io.open(file, "a")
   io.output(f)
   io.write("hello world\n")
   io.close(f)
end

exgest("example.txt")