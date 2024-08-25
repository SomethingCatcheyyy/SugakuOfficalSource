using UnityEngine;
using System.Collections;

public class SpriteRotatorCustom : MonoBehaviour
{

	public int offset;

	// Token: 0x06000990 RID: 2448 RVA: 0x00008B78 File Offset: 0x00006D78
	private void Start()
	{
		this.angleRange = (float)(360 / this.sprites.Length);
		this.cam = Camera.main.transform;
	}

	// Token: 0x06000991 RID: 2449 RVA: 0x00039E68 File Offset: 0x00038068
	private void Update()
	{
		if (cam == null) {
			cam = Camera.main.transform;
		} else {
			float num = Mathf.Atan2 (this.cam.position.z - base.transform.position.z, this.cam.position.x - base.transform.position.x) * 57.29578f;
			num += base.transform.eulerAngles.y;
			if (num < 0f) {
				num += 360f;
			} else if (num >= 360f) {
				num -= 360f;
			}
			int num2 = Mathf.RoundToInt (num / this.angleRange);
			while (num2 < 0 || num2 >= this.sprites.Length) {
				num2 += (int)((float)(-1 * this.sprites.Length) * Mathf.Sign ((float)num2));
			}
			if (spriteRenderer != null && sprites != null) {
				this.spriteRenderer.sprite = this.sprites [(num2 + offset) % sprites.Length];
			}
		}
	}

	// Token: 0x040009A5 RID: 2469
	private Transform cam;

	public SpriteRenderer spriteRenderer;

	public Sprite[] sprites = new Sprite[0];

	// Token: 0x040009A8 RID: 2472
	private float angleRange;
}