import { NextResponse } from "next/server";

export async function POST(req: Request) {
  try {
    const { messages } = await req.json();
    const lastMessage = messages[messages.length - 1];
    
    let reply = "I'm a demo chatbot. I can help you find products or answer simple questions about our shop!";
    
    if (lastMessage.content.toLowerCase().includes("product") || lastMessage.content.toLowerCase().includes("recommend")) {
      reply = "I recommend checking out our latest electronics! You can also visit the Recommendations page for more personalized suggestions.";
    } else if (lastMessage.content.toLowerCase().includes("shipping")) {
      reply = "We offer free shipping on all orders over 500,000₫ and free 0₫ returns!";
    }

    return NextResponse.json({
      role: "assistant",
      content: reply,
    });
  } catch (error) {
    console.error(error);
    return NextResponse.json(
      { error: "Failed to process chat message" },
      { status: 500 }
    );
  }
}
