import { NextResponse } from "next/server";

export async function POST(req: Request) {
  try {
    const { message, history } = await req.json();

    // Simulate some delay for AI processing
    await new Promise((resolve) => setTimeout(resolve, 1000));

    const lower = message.toLowerCase();
    let response = "";

    if (lower.includes("recommend") || lower.includes("product")) {
      response = "Based on our latest trends, I recommend checking out our premium wireless headphones and eco-friendly water bottles. Would you like to see more details?";
    } else if (lower.includes("track") || lower.includes("order")) {
      response = "I can help with that! Please provide your order number, or you can find the tracking status directly in your 'Purchases' dashboard.";
    } else if (lower.includes("shipping")) {
      response = "We offer standard shipping (3-5 days) and express shipping (1-2 days). Orders over $50 qualify for free standard shipping!";
    } else if (lower.includes("return") || lower.includes("refund")) {
      response = "Our return policy allows for returns within 30 days of purchase. The items must be in their original condition. You can start a return in your account settings.";
    } else if (lower.includes("contact") || lower.includes("support") || lower.includes("human")) {
      response = "You can reach our live support team at support@shopbee.com or call us at 1-800-SHOPBEE (Mon-Fri, 9am-6pm EST).";
    } else if (lower.includes("hello") || lower.includes("hi")) {
      response = "Hello! I'm your Shopbee AI assistant. How can I help you today?";
    } else {
      response = "That's interesting! I'm learning more every day. Could you tell me more about that, or are you looking for help with an order or product recommendation?";
    }

    return NextResponse.json({ response });
  } catch (error) {
    console.error("Chat API error:", error);
    return NextResponse.json(
      { error: "Failed to process chat message" },
      { status: 500 }
    );
  }
}
