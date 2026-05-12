export function mapComponent(c) {
  return {
    id: c.id ?? c.Id,
    name: c.name ?? c.Name,
    category: c.category ?? c.Category,
    price: c.price ?? c.Price,
    currency: c.currency ?? c.Currency,
    specifications: c.specifications ?? c.Specifications,
    socket: c.socket ?? c.Socket,
    powerConsumption: c.powerConsumption ?? c.PowerConsumption,
    imageUrl: c.imageUrl ?? c.ImageUrl
  }
}