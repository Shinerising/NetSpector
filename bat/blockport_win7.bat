netsh ipsec static delete all
netsh ipsec static add filterlist name=BLOCK
netsh ipsec static add filter filterlist=BLOCK srcaddr=any dstaddr=Me protocol=tcp srcport=0 dstport=445
netsh ipsec static add filter filterlist=BLOCK srcaddr=any dstaddr=Me protocol=udp srcport=0 dstport=445
netsh ipsec static add filteraction name=ACTION_BLOCK action=block
netsh ipsec static add policy name=POLICY_BLOCK assign=yes
netsh ipsec static add rule name=RULE_BLOCK policy=POLICY_BLOCK filterlist=BLOCK filteraction=ACTION_BLOCK