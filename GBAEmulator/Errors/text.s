.global init_text
.global putchar
.global write
.global print_hex


.data

// 1bpp encoding
font_tiles:

// font taken from https://www.coranac.com/tonc/text/text.htm

.word 0x00000000, 0x00000000, 0x18181818, 0x00180018, 0x00003636, 0x00000000, 0x367F3636, 0x0036367F
.word 0x3C067C18, 0x00183E60, 0x1B356600, 0x0033566C, 0x6E16361C, 0x00DE733B, 0x000C1818, 0x00000000
.word 0x0C0C1830, 0x0030180C, 0x3030180C, 0x000C1830, 0xFF3C6600, 0x0000663C, 0x7E181800, 0x00001818
.word 0x00000000, 0x0C181800, 0x7E000000, 0x00000000, 0x00000000, 0x00181800, 0x183060C0, 0x0003060C 
.word 0x7E76663C, 0x003C666E, 0x181E1C18, 0x00181818, 0x3060663C, 0x007E0C18, 0x3860663C, 0x003C6660 
.word 0x33363C38, 0x0030307F, 0x603E067E, 0x003C6660, 0x3E060C38, 0x003C6666, 0x3060607E, 0x00181818
.word 0x3C66663C, 0x003C6666, 0x7C66663C, 0x001C3060, 0x00181800, 0x00181800, 0x00181800, 0x0C181800 
.word 0x06186000, 0x00006018, 0x007E0000, 0x0000007E, 0x60180600, 0x00000618, 0x3060663C, 0x00180018 
.word 0x5A5A663C, 0x003C067A, 0x7E66663C, 0x00666666, 0x3E66663E, 0x003E6666, 0x06060C78, 0x00780C06 
.word 0x6666361E, 0x001E3666, 0x1E06067E, 0x007E0606, 0x1E06067E, 0x00060606, 0x7606663C, 0x007C6666 
.word 0x7E666666, 0x00666666, 0x1818183C, 0x003C1818, 0x60606060, 0x003C6660, 0x0F1B3363, 0x0063331B 
.word 0x06060606, 0x007E0606, 0x6B7F7763, 0x00636363, 0x7B6F6763, 0x00636373, 0x6666663C, 0x003C6666 
.word 0x3E66663E, 0x00060606, 0x3333331E, 0x007E3B33, 0x3E66663E, 0x00666636, 0x3C0E663C, 0x003C6670
.word 0x1818187E, 0x00181818, 0x66666666, 0x003C6666, 0x66666666, 0x00183C3C, 0x6B636363, 0x0063777F 
.word 0x183C66C3, 0x00C3663C, 0x183C66C3, 0x00181818, 0x0C18307F, 0x007F0306, 0x0C0C0C3C, 0x003C0C0C 
.word 0x180C0603, 0x00C06030, 0x3030303C, 0x003C3030, 0x00663C18, 0x00000000, 0x00000000, 0x003F0000 
.word 0x00301818, 0x00000000, 0x603C0000, 0x007C667C, 0x663E0606, 0x003E6666, 0x063C0000, 0x003C0606 
.word 0x667C6060, 0x007C6666, 0x663C0000, 0x003C067E, 0x0C3E0C38, 0x000C0C0C, 0x667C0000, 0x3C607C66
.word 0x663E0606, 0x00666666, 0x18180018, 0x00301818, 0x30300030, 0x1E303030, 0x36660606, 0x0066361E
.word 0x18181818, 0x00301818, 0x7F370000, 0x0063636B, 0x663E0000, 0x00666666, 0x663C0000, 0x003C6666 
.word 0x663E0000, 0x06063E66, 0x667C0000, 0x60607C66, 0x663E0000, 0x00060606, 0x063C0000, 0x003E603C
.word 0x0C3E0C0C, 0x00380C0C, 0x66660000, 0x007C6666, 0x66660000, 0x00183C66, 0x63630000, 0x00367F6B 
.word 0x36630000, 0x0063361C, 0x66660000, 0x0C183C66, 0x307E0000, 0x007E0C18, 0x0C181830, 0x00301818
.word 0x18181818, 0x00181818, 0x3018180C, 0x000C1818, 0x003B6E00, 0x00000000, 0x00000000, 0x00000000

font_tiles_end:



font_tiles_size = font_tiles_end - font_tiles


.arm
.text

/*
	void no args
*/
init_text:

   	push {r0-r7}

	// dispcnt enable bg1 text mode 0
	mov r0, #0x04000000
	mov r1, #0x100
	strh r1, [r0]

	# bg1cnt 4bpp max priority
	# relocate bg map
	mov r1, #0x800
	strh r1, [r0,#0x8]

	# setup palette
	mov r0, #0x05000000
	// yeah i know we only use the bottom of this
	mov r1, #0xffffffff 
	strh r1, [r0,#2]


	mov r0, #0x06000000
	ldr r1, =font_tiles_size
	ldr r2, =font_tiles 


	// 1bpp packing to 4bpp
	// so take a byte and for each byte
	// unpack 2 bits into a nibble
	// store in vram

tile_copy_loop:
	// load our tile data
	ldrb r3, [r2,r1]

	// we load two bits at a time out of this 
	// so 4 instead of 8
	mov r4, #4 

	# reset our reg we are decoding the data into
	mov r5, #0
unpack_loop:
	// shift up our decoded data
	lsl r5, #8

	// load low nibble
	and r6, r3, #128
	lsr r6, #7
	lsl r3, #1
	lsl r6, #4

	// load high nibble 
	and r7, r3, #128
	lsr r7, #7
	lsl r3, #1

	# combine current byte into word
	orr r6, r7
	orr r5, r6


	// if we still have data in the byte keep doing
	subs r4, #0x1
	bne unpack_loop

	// unpack is done our decoded 
	// 4bpp tile should be in r4
	// need to *4 r0 as our output 
	// is 4 times larger
	str r5, [r0,r1, lsl #0x2]


	subs r1, #1
	bne tile_copy_loop


	// reset our text counters
	// how to define this as a constant?
	mov r0, #0x02000000
	strb r1, [r0] // col 
	strb r1, [r0,#1] // row

    pop {r0-r7}
    bx lr


// r0: char to print
// void return
putchar:
	push {r0-r5}

	// get bg col and row
	mov r2, #0x02000000
	ldrb r3, [r2]
	ldrb r4, [r2,#1]

	// load bg map offset
	ldr r1, =#0x06004000

	// check for linefeed
	cmp r0, #0xa
	beq inc_row




	// if in the printable ascii range
	cmp r0, #0x20
	// ignore non printable chars
	blt putchar_end

	cmp r0, #0x7e
	bgt putchar_end

	// convert to printable range
	sub r0, #0x20


	// calc bg offset

	// calc col offset
	mov r5, r3, lsl #1

	// calc row offset
	add r5, r4, lsl #6



	// store the char
	strh r0, [r5,r1]

	// inc row and check we havent gone past screen size
	add r3, #1
	cmp r3, #30

	bne store_cords

inc_row:
	// reset col and goto next row
	mov r3, #0
	add r4,#1

	// end of screen copy each one up
	cmp r4, #20
	bne store_cords

	// bg line size
	mov r2, #64

	// transfer limit
	add r3, r1, #0x500

	// r0 dst, r1 source
	mov r0, r1
	add r1, r2

row_copy_loop:
	push {lr}
	bl memcpy
	pop {lr}

	// inc dst, src
	add r0, r2
	add r1, r2

	// if src is at line 20 we are done
	cmp r0, r3
	bne row_copy_loop

	// last line col zero
	mov r3, #0
	mov r4, #19
	mov r2, #0x02000000
	


store_cords:
	strb r3, [r2]
	strb r4, [r2,#1]

putchar_end:
	pop {r0-r5}
	bx lr 


// r0: pointer to string
// void return
write:
	push {r0-r1}

	mov r1, r0

	// while not zero putchar
write_loop:
	ldrb r0, [r1], #1
	cmp r0, #0x0
	beq write_done
	push {lr}
	bl putchar
	pop {lr}
	b write_loop 
	

write_done:
	pop {r0-r1}
	bx lr 




// r0 number to print 
// void return
print_hex:
	push {r0-r2}


	// get each nibble of the arg
	// and convert it to a char
	mov r1, r0

	mov r2, #8
hex_loop:
	and r0, r1, #0xf0000000
	lsr r0, #28
	lsl r1, #4
	cmp r0, #9
	bgt conv_upper_hex
	// convert to a number
	add r0, #48
	b hex_printchar

conv_upper_hex:
	// this now above ten so convert to 'a' - 'f'
	add r0, #55


hex_printchar:
	push {lr}
	bl putchar
	pop {lr}

	subs r2, #1
	bne hex_loop

	pop {r0-r2}
	bx lr


// need to these to handle bus limitations
// otherwhise they will not copy properly



// currently wont handle a odd transfer size...

// r0 dest 
// r1 src
// r2 size
// void return
memcpy:
	push {r2-r3}

	// ignore 0th bit
	mvn r3, #1
	and r2, r3
memcpy_loop:
	ldrh r3, [r1,r2]
	strh r3, [r0,r2]
	subs r2, #2
	bne memcpy_loop

	pop {r2-r3}
	bx lr


// r0 dest
// r1 val
// r2 size
memset:
	push {r2}
memset_loop:
	strb r1, [r0, r2]
	subs r2, #1
	bne memset_loop

	pop {r2}
	bx lr