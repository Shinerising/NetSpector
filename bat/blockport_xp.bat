ipseccmd -u
ipseccmd -w REG -p "BLOCK" -r "Block TCP/445" -f *+0:445:TCP -n BLOCK -y
ipseccmd -w REG -p "BLOCK" -r "Block UDP/445" -f *+0:445:UDP -n BLOCK -y
ipseccmd -w REG -p "BLOCK" -r "Block TCP/445" -f *+0:445:TCP -n BLOCK -x
ipseccmd -w REG -p "BLOCK" -r "Block UDP/445" -f *+0:445:UDP -n BLOCK -x