export function mapComponent(c) {
  return {
    id: c.Id ?? c.id,
    name: c.Name ?? c.name,
    category: c.Category ?? c.category,
    price: c.Price ?? c.price,
    currency: c.Currency ?? c.currency,
    specifications: c.Specifications ?? c.specifications,
    socket: c.Socket ?? c.socket,
    powerConsumption: c.PowerConsumption ?? c.powerConsumption,
    imageUrl: c.ImageUrl ?? c.imageUrl
  }
}

